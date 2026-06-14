using System.Net.Http.Json;
using System.Text.Json;
using HealthCare.Application.DTOs.ML;
using HealthCare.Application.Interfaces;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HealthCare.Infreastructure.Services;

public class MLService : IMLService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _http;
    private readonly ILogger<MLService> _logger;
    private readonly string _mlBaseUrl;


private const double FixedHeartRate = 70.0;

    public MLService(
        AppDbContext db,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<MLService> logger)
    {
        _db = db;
        _http = httpClientFactory.CreateClient("MLService");
        _logger = logger;

        _mlBaseUrl = configuration["MLService:BaseUrl"]
                     ?? throw new InvalidOperationException(
                         "MLService:BaseUrl is not configured in appsettings.json");
    }

    public async Task<VitalsListDto> GetVitalsForPredictionAsync(Guid patientId)
    {
        _logger.LogInformation("Fetching vitals for patient/user {PatientId}", patientId);

        var vitals = await _db.Measurements
            .Where(m => m.Device != null && m.Device.UserId == patientId)
            .OrderByDescending(m => m.RecordedAt)
            .Select(m => new
            {
                m.bpm,
                m.spo2,
                m.resp_rate,
                m.temp
            })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"No vitals found for patient/user {patientId}");

        var dto = new VitalsListDto
        {
            Vitals = new List<double>
        {
            FixedHeartRate,
            vitals.bpm,
            vitals.spo2,
            vitals.resp_rate,
            vitals.temp
        }
        };

        _logger.LogInformation(
            "Vitals list built for patient/user {PatientId}: [{Vitals}]",
            patientId,
            string.Join(", ", dto.Vitals));

        return dto;
    }

    public async Task<PredictionResultDto> PredictHealthRiskAsync(VitalsListDto vitals)
    {
        _logger.LogInformation(
            "Sending vitals to ML service: [{Vitals}]",
            string.Join(", ", vitals.Vitals));

        var predictUrl = $"{_mlBaseUrl.TrimEnd('/')}/predict";

        var response = await _http.PostAsJsonAsync(predictUrl, vitals);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();

            _logger.LogError(
                "ML service returned {StatusCode}: {Body}",
                response.StatusCode,
                body);

            throw new HttpRequestException(
                $"ML service error {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<PredictionResultDto>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            ?? throw new InvalidOperationException("ML service returned null response");

        _logger.LogInformation("ML prediction result: {Status}", result.Status);

        return result;
    }


}
