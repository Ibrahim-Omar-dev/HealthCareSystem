using HealthCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlertController : BaseApiController
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
        public async Task<IActionResult> GetCritical([FromQuery] Guid? userId = null)
        {
            Guid resolvedUserId;

            if (userId.HasValue)
            {
                resolvedUserId = userId.Value;
            }
            else
            {
                if (!TryGetCurrentUserId(out resolvedUserId, out IActionResult? error))
                    return error!;
            }

            var alerts = await _alertService.GetCriticalAlertsAsync(resolvedUserId);
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

        
    }
}