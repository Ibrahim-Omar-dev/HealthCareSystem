using HealthCare.Application.Dto;
using HealthCare.Application.Interfaces;
using HealthCare.Domain.Interface;
using HealthCare.Infreastructure.Services.Interface.IMeasurement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class MeasurementController : ControllerBase
    {
        private readonly IUserManagement _userManagement;
        private readonly IMeasurementService _measurementService;
        private readonly IAlertService _alertService;

        public MeasurementController(
            IUserManagement userManagement,
            IMeasurementService measurementService,IAlertService alertService)
        {
            _userManagement = userManagement;
            _measurementService = measurementService;
            _alertService = alertService;
        }

        [HttpGet("GetAllData")]
        public async Task<IActionResult> GetAllData()
        {
            var data = await _measurementService.GetAllDataAsync();

            if (!data.Any())
                return NotFound("No data found.");

            return Ok(data);
        }

        [HttpGet("GetDataByUserId/{userId:guid}")]
        public async Task<IActionResult> GetDataByUserId(Guid userId)
        {
            var data = await _measurementService.GetDataByUserIdAsync(userId);

            if (!data.Any())
                return NotFound("No data found for the given user.");

            return Ok(data);
        }

        [HttpGet("GetMyData")]
        public async Task<IActionResult> GetMyData()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var data = await _measurementService.GetMyDataAsync(userId);

            if (!data.Any())
                return NotFound("No measurements found for this user.");

            return Ok(data);
        }

        [HttpPost("AddData")]
        [AllowAnonymous]
        public async Task<IActionResult> AddData(SensorMeasurementDto dto)
        {
            Guid? userId = null;

            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(claim) && Guid.TryParse(claim, out Guid parsed))
                userId = parsed;

            var measurement = await _measurementService.AddDataAsync(dto, userId);
            if (userId.HasValue)
                await _alertService.GenerateAlertsFromMeasurementAsync(measurement);

            return Ok(new
            {
                success = true,
                isAuthenticated = userId != null,
                message = "Measurement added successfully"
            });
        }


        /// <summary>Returns the latest measurement recorded in the last 6 hours.</summary>
        [HttpGet("GetDataLast6Hours")]
        public async Task<IActionResult> GetDataLast6Hours()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var record = await _measurementService.GetLatestInLast6HoursAsync(userId);

            return record is null
                ? NotFound("No measurement found in the last 6 hours.")
                : Ok(record);
        }

        /// <summary>Returns the latest measurement recorded in the last 12 hours.</summary>
        [HttpGet("GetDataLast12Hours")]
        public async Task<IActionResult> GetDataLast12Hours()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var record = await _measurementService.GetLatestInLast12HoursAsync(userId);

            return record is null
                ? NotFound("No measurement found in the last 12 hours.")
                : Ok(record);
        }

        /// <summary>Returns the latest measurement recorded in the last 24 hours.</summary>
        [HttpGet("GetDataLast24Hours")]
        public async Task<IActionResult> GetDataLast24Hours()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var record = await _measurementService.GetLatestInLast24HoursAsync(userId);

            return record is null
                ? NotFound("No measurement found in the last 24 hours.")
                : Ok(record);
        }

        /// <summary>Returns the latest measurement recorded in the last 3 days.</summary>
        [HttpGet("GetDataLast3Days")]
        public async Task<IActionResult> GetDataLast3Days()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var record = await _measurementService.GetLatestInLast3DaysAsync(userId);

            return record is null
                ? NotFound("No measurement found in the last 3 days.")
                : Ok(record);
        }

        /// <summary>Returns the very last measurement recorded by the current user.</summary>
        [HttpGet("GetLastRecord")]
        public async Task<IActionResult> GetLastRecord()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var record = await _measurementService.GetLastRecordAsync(userId);

            return record is null
                ? NotFound("No measurements found for this user.")
                : Ok(record);
        }


        /// <summary>
        /// Tries to parse the authenticated user's ID from the JWT claims.
        /// Returns false and sets <paramref name="error"/> when extraction fails.
        /// </summary>
        private bool TryGetCurrentUserId(out Guid userId, out IActionResult? error)
        {
            userId = Guid.Empty;
            error = null;

            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(claim))
            {
                error = Unauthorized("User ID not found in token.");
                return false;
            }

            if (!Guid.TryParse(claim, out userId))
            {
                error = BadRequest("Invalid User ID format.");
                return false;
            }

            return true;
        }
    }
}