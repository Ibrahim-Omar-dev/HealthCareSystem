using HealthCare.Application.DTOs.ML;
using HealthCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthCare.Presentation.Controllers;

[ApiController]
[Route("api/ml")]
[Authorize]
public class MLController : ControllerBase
{
    private readonly IMLService _mlService;
    private readonly ILogger<MLController> _logger;


public MLController(IMLService mlService, ILogger<MLController> logger)
    {
        _mlService = mlService;
        _logger = logger;
    }

    // GET api/ml/vitals/me
    // Gets current logged-in user's vitals from token UserId
    [HttpGet("vitals/me")]
    [ProducesResponseType(typeof(VitalsListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVitalsForCurrentUser()
    {
        if (!TryGetCurrentUserId(out var userId, out var error))
            return error!;

        try
        {
            var result = await _mlService.GetVitalsForPredictionAsync(userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "No vitals found for current user {UserId}", userId);

            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // POST api/ml/predict/me
    // Full pipeline for current logged-in user:
    // Token UserId -> DB measurements -> Python ML service -> prediction result
    [HttpPost("predict/me")]
    [ProducesResponseType(typeof(PredictionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> PredictForCurrentUser()
    {
        if (!TryGetCurrentUserId(out var userId, out var error))
            return error!;

        try
        {
            var vitals = await _mlService.GetVitalsForPredictionAsync(userId);
            var prediction = await _mlService.PredictHealthRiskAsync(vitals);

            return Ok(prediction);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "No vitals found for current user {UserId}", userId);

            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "ML service unreachable or returned an error");

            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "ML service is unavailable. Please make sure the Python service is running."
            });
        }
    }

    // GET api/ml/vitals/{patientId}
    // Returns: { "vitals": [70, bpm, spo2, resp_rate, temp] }
    [HttpGet("vitals/{patientId:guid}")]
    [ProducesResponseType(typeof(VitalsListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVitals(Guid patientId)
    {
        try
        {
            var result = await _mlService.GetVitalsForPredictionAsync(patientId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "No vitals found for patient/user {PatientId}", patientId);

            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // POST api/ml/predict/{patientId}
    // Full pipeline: DB measurements -> Python ML service -> prediction result
    [HttpPost("predict/{patientId:guid}")]
    [ProducesResponseType(typeof(PredictionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Predict(Guid patientId)
    {
        try
        {
            var vitals = await _mlService.GetVitalsForPredictionAsync(patientId);
            var prediction = await _mlService.PredictHealthRiskAsync(vitals);

            return Ok(prediction);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "No vitals found for patient/user {PatientId}", patientId);

            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "ML service unreachable or returned an error");

            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "ML service is unavailable. Please make sure the Python service is running."
            });
        }
    }

    // POST api/ml/predict/direct
    // Testing only: send vitals directly without DB
    // Body: { "vitals": [70, 120, 98, 16, 36.6] }
    [HttpPost("predict/direct")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PredictionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> PredictDirect([FromBody] VitalsListDto dto)
    {
        if (dto == null || dto.Vitals == null || dto.Vitals.Count != 5)
        {
            return BadRequest(new
            {
                message = "vitals must contain exactly 5 values: [heart_rate, blood_pressure, oxygen_saturation, respiratory_rate, temperature]"
            });
        }

        try
        {
            var prediction = await _mlService.PredictHealthRiskAsync(dto);
            return Ok(prediction);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "ML service unreachable or returned an error");

            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "ML service is unavailable. Please make sure the Python service is running."
            });
        }
    }

    private bool TryGetCurrentUserId(out Guid userId, out IActionResult? error)
    {
        userId = Guid.Empty;
        error = null;

        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(claim))
        {
            error = Unauthorized(new
            {
                message = "User ID not found in token."
            });

            return false;
        }

        if (!Guid.TryParse(claim, out userId))
        {
            error = BadRequest(new
            {
                message = "Invalid User ID format."
            });

            return false;
        }

        return true;
    }


}
