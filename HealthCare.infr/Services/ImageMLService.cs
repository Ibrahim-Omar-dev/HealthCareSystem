using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HealthCare.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HealthCare.Application.Dto.ImageML;

namespace HealthCare.Infreastructure.Services;

public class ImageMLService : IImageMLService
{
    private readonly HttpClient _http;
    private readonly ILogger<ImageMLService> _logger;
    private readonly string _baseUrl;

    public ImageMLService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ImageMLService> logger)
    {
        _http = httpClientFactory.CreateClient("ImageMLService");
        _logger = logger;

        _baseUrl = configuration["HuggingFaceImageML:BaseUrl"]
                   ?? throw new InvalidOperationException(
                       "HuggingFaceImageML:BaseUrl is not configured");
    }

    public async Task<ImagePredictionResultDto> PredictAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // 1) Upload image
        var uploadedPath = await UploadImageAsync(
            imageStream,
            fileName,
            contentType,
            cancellationToken);

        // 2) Start prediction and get event_id
        var eventId = await StartPredictionAsync(uploadedPath, cancellationToken);

        // 3) Get final result from event stream
        var rawResult = await GetPredictionResultAsync(eventId, cancellationToken);

        return new ImagePredictionResultDto
        {
            Success = true,
            UploadedPath = uploadedPath,
            EventId = eventId,
            RawResult = rawResult
        };
    }

    private async Task<string> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent();

        var fileContent = new StreamContent(imageStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType);

        form.Add(fileContent, "files", fileName);

        var response = await _http.PostAsync(
            $"{_baseUrl.TrimEnd('/')}/gradio_api/upload",
            form,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Image upload failed. Status: {Status}. Body: {Body}",
                response.StatusCode,
                body);

            throw new HttpRequestException(
                $"Image upload failed {(int)response.StatusCode}: {body}");
        }

        var uploadResult = JsonSerializer.Deserialize<List<string>>(body);

        if (uploadResult == null || uploadResult.Count == 0)
        {
            throw new InvalidOperationException(
                $"Upload response does not contain uploaded path. Response: {body}");
        }

        return uploadResult[0];
    }

    private async Task<string> StartPredictionAsync(
        string uploadedPath,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            img = new
            {
                path = uploadedPath,
                meta = new
                {
                    _type = "gradio.FileData"
                }
            }
        };

        var response = await _http.PostAsJsonAsync(
            $"{_baseUrl.TrimEnd('/')}/gradio_api/call/v2/ensemble_predict",
            payload,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Image prediction call failed. Status: {Status}. Body: {Body}",
                response.StatusCode,
                body);

            throw new HttpRequestException(
                $"Image prediction call failed {(int)response.StatusCode}: {body}");
        }

        using var json = JsonDocument.Parse(body);

        if (!json.RootElement.TryGetProperty("event_id", out var eventIdElement))
        {
            throw new InvalidOperationException(
                $"Prediction response does not contain event_id. Response: {body}");
        }

        var eventId = eventIdElement.GetString();

        if (string.IsNullOrWhiteSpace(eventId))
        {
            throw new InvalidOperationException(
                $"event_id is empty. Response: {body}");
        }

        return eventId;
    }

    private async Task<string> GetPredictionResultAsync(
        string eventId,
        CancellationToken cancellationToken)
    {
        var response = await _http.GetAsync(
            $"{_baseUrl.TrimEnd('/')}/gradio_api/call/ensemble_predict/{eventId}",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Get image prediction result failed. Status: {Status}. Body: {Body}",
                response.StatusCode,
                body);

            throw new HttpRequestException(
                $"Get image prediction result failed {(int)response.StatusCode}: {body}");
        }

        return ExtractFinalDataFromSse(body);
    }

    private static string ExtractFinalDataFromSse(string sseBody)
    {
        if (string.IsNullOrWhiteSpace(sseBody))
            return string.Empty;

        var dataLines = sseBody
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            .Select(line => line.Substring("data:".Length).Trim())
            .ToList();

        if (dataLines.Count == 0)
            return sseBody;

        return dataLines.Last();
    }
}