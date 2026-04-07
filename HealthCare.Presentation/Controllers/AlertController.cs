using HealthCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlertController : ControllerBase
    {
        private readonly IAlertService _alertService;

        public AlertController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet("GetMyAlerts")]
        public async Task<IActionResult> GetMyAlerts()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var alerts = await _alertService.GetMyAlertsAsync(userId);
            return Ok(alerts);
        }

        [HttpGet("GetUnread")]
        public async Task<IActionResult> GetUnread()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var alerts = await _alertService.GetUnreadAlertsAsync(userId);
            return Ok(alerts);
        }

        [HttpGet("GetCritical")]
        public async Task<IActionResult> GetCritical()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var alerts = await _alertService.GetCriticalAlertsAsync(userId);
            return Ok(alerts);
        }

        [HttpPut("MarkAsRead/{alertId:guid}")]
        public async Task<IActionResult> MarkAsRead(Guid alertId)
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            await _alertService.MarkAsReadAsync(alertId, userId);
            return Ok(new { success = true });
        }

        [HttpPut("MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            await _alertService.MarkAllAsReadAsync(userId);
            return Ok(new { success = true });
        }

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