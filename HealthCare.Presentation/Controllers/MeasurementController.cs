using HealthCare.Application.Dto;
using HealthCare.Application.Interfaces;
using HealthCare.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MeasurementController : BaseApiController
    {
        private readonly IMeasurementService _measurementService;

        public MeasurementController(IMeasurementService measurementService)
        {
            _measurementService = measurementService;
        }


        [HttpGet("GetAllData")]
        public async Task<IActionResult> GetAllData()
        {
            var data = await _measurementService.GetAllDataAsync();

            if (!data.Any())
                return NotFound(new { success = false, message = "No data found." });

            return Ok(data);
        }


        [HttpGet("GetMyData")]
        public async Task<IActionResult> GetMyData()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var data = await _measurementService.GetMyDataAsync(userId);

            if (!data.Any())
                return NotFound(new { success = false, message = "No measurements found." });

            return Ok(data);
        }


        [HttpGet("GetUserData/{targetUserId:guid}")]
        public async Task<IActionResult> GetUserData(Guid targetUserId)
        {
            if (!TryGetCurrentUserId(out Guid requesterId, out IActionResult? error))
                return error!;

            if (requesterId == targetUserId)
            {
                var own = await _measurementService.GetMyDataAsync(requesterId);
                if (!own.Any())
                    return NotFound(new { success = false, message = "No measurements found." });
                return Ok(own);
            }

            var data = await _measurementService.GetUserDataAsync(requesterId, targetUserId);

            if (data is null)
                return StatusCode(403, new
                {
                    success = false,
                    message = "Access denied. You must be an accepted follower."
                });

            if (!data.Any())
                return NotFound(new { success = false, message = "No measurements found for this user." });

            return Ok(data);
        }


        [HttpPost("AddData")]
        [AllowAnonymous]
        public async Task<IActionResult> AddData([FromBody] SensorMeasurementDto dto)
        {
            var result = await _measurementService.AddDataAsync(dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }


        [HttpGet("GetLastRecord")]
        public async Task<IActionResult> GetLastRecord([FromQuery] Guid? targetUserId = null)
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var resolvedId = targetUserId ?? userId;

            if (resolvedId != userId)
            {
                var allowed = await _measurementService.IsFollowingAsync(userId, resolvedId);
                if (!allowed)
                    return StatusCode(403, new { success = false, message = "Access denied." });
            }

            var record = await _measurementService.GetLastRecordAsync(resolvedId);

            return record is null
                ? NotFound(new { success = false, message = "No measurements found." })
                : Ok(record);
        }

        [HttpGet("GetDataLast6Hours")]
        public async Task<IActionResult> GetDataLast6Hours([FromQuery] Guid? targetUserId = null)
            => await GetRangeData(targetUserId, _measurementService.GetDataInLast6HoursAsync, "6 hours");

        [HttpGet("GetDataLast12Hours")]
        public async Task<IActionResult> GetDataLast12Hours([FromQuery] Guid? targetUserId = null)
            => await GetRangeData(targetUserId, _measurementService.GetDataInLast12HoursAsync, "12 hours");

        [HttpGet("GetDataLast24Hours")]
        public async Task<IActionResult> GetDataLast24Hours([FromQuery] Guid? targetUserId = null)
            => await GetRangeData(targetUserId, _measurementService.GetDataInLast24HoursAsync, "24 hours");

        [HttpGet("GetDataLast3Days")]
        public async Task<IActionResult> GetDataLast3Days([FromQuery] Guid? targetUserId = null)
            => await GetRangeData(targetUserId, _measurementService.GetDataInLast3DaysAsync, "3 days");

        [HttpGet("GetDataLast7Days")]
        public async Task<IActionResult> GetDataLast7Days([FromQuery] Guid? targetUserId = null)
            => await GetRangeData(targetUserId, _measurementService.GetDataInLast7DaysAsync, "7 days");

        private async Task<IActionResult> GetRangeData(
            Guid? targetUserId,
            Func<Guid, Task<SensorMeasurement?>> serviceCall,
            string rangeLabel)
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var resolvedId = targetUserId ?? userId;

            if (resolvedId != userId)
            {
                var allowed = await _measurementService.IsFollowingAsync(userId, resolvedId);
                if (!allowed)
                    return StatusCode(403, new { success = false, message = "Access denied." });
            }

            var record = await serviceCall(resolvedId);

            return record is null
                ? NotFound(new
                {
                    success = false,
                    message = $"No measurement found in the last {rangeLabel}."
                })
                : Ok(record);
        }

       
    }
}