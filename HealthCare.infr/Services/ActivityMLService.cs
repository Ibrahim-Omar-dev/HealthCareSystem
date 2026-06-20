using HealthCare.Application.Dto.Accelerometer;
using HealthCare.Application.Services.Interfaces;
using HealthCare.Domain.Entities;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using HealthCare.Application.DTOs.ActivityML;
using HealthCare.Application.Interfaces;

namespace HealthCare.Infrastructure.Services;

public class ActivityMLService : IActivityMLService
{
    private readonly HttpClient _httpClient;

    public ActivityMLService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ActivityMLService");
    }

    public async Task<ActivityPredictionResultDto> PredictAsync(
        List<List<double>> readings,
        CancellationToken cancellationToken = default)
    {
        if (readings == null || readings.Count == 0)
            throw new ArgumentException("Accelerometer readings are required.");

        var response = await _httpClient.PostAsJsonAsync(
            "/predict",
            new { readings },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Activity ML service error: {response.StatusCode} - {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<ActivityPredictionResultDto>(
            cancellationToken: cancellationToken);

        if (result == null)
            throw new InvalidOperationException("Activity ML service returned empty response.");

        return result;
    }
}