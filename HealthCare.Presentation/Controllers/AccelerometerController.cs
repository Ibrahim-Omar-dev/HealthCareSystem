using HealthCare.Application.Dto.Accelerometer;
using HealthCare.Application.Interfaces;
using HealthCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccelerometerController : BaseApiController
    {
        private readonly IAccelerometerService _accelerometerService;
        private readonly IActivityMLService _activityMLService;

        public AccelerometerController(
            IAccelerometerService accelerometerService,
            IActivityMLService activityMLService)
        {
            _accelerometerService = accelerometerService;
            _activityMLService = activityMLService;
        }

        [HttpPost("AddReadings")]
        [AllowAnonymous]
        public async Task<IActionResult> AddReadings([FromBody] AddAccelerometerDto dto)
        {
            var (success, message) = await _accelerometerService.AddReadingsAsync(dto);

            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });
        }

        [HttpGet("GetAll")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var readings = await _accelerometerService.GetMyReadingsAsync();
            return Ok(readings);
        }

        [HttpPost("PredictActivity")]
        [Authorize]
        public async Task<IActionResult> PredictActivity()
        {
            try
            {
                var recordsResult = await _accelerometerService.GetMyReadingsAsync();

                if (recordsResult == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No accelerometer records found."
                    });
                }

                if (recordsResult is not IEnumerable enumerableRecords)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Accelerometer records data is not a valid list."
                    });
                }

                var records = enumerableRecords
                    .Cast<AccelerometerResponseDto>()
                    .ToList();

                if (records.Count == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No accelerometer records found."
                    });
                }

                var latestRecord = records
                    .OrderByDescending(r => r.RecordedAt)
                    .FirstOrDefault();

                if (latestRecord == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No accelerometer records found."
                    });
                }

                if (latestRecord.Readings == null || latestRecord.Readings.Count == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No readings found inside the latest accelerometer record."
                    });
                }

                const int MaxReadings = 200;

                var latestReadings = latestRecord.Readings.Count > MaxReadings
                    ? latestRecord.Readings
                        .Skip(latestRecord.Readings.Count - MaxReadings)
                        .Take(MaxReadings)
                        .ToList()
                    : latestRecord.Readings;

                var validReadings = latestReadings
                    .Where(r => r != null && r.Count >= 3)
                    .Select(r => new List<double>
                    {
                        r[0],
                        r[1],
                        r[2]
                    })
                    .ToList();

                if (validReadings.Count == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No valid XYZ readings found. Each reading must contain [x, y, z]."
                    });
                }

                var result = await _activityMLService.PredictAsync(
                    validReadings,
                    HttpContext.RequestAborted);

                return Ok(new
                {
                    success = true,
                    activity = result.Activity,
                    confidence = result.Confidence,
                    readingsCount = validReadings.Count
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    success = false,
                    message = "Activity ML service is unavailable or returned an error.",
                    details = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Unexpected error while predicting activity.",
                    details = ex.Message
                });
            }
        }    }
}
