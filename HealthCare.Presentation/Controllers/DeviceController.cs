using HealthCare.Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DeviceController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpPost("link-device")]
        public async Task<IActionResult> LinkDevice([FromBody] LinkDeviceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceCode))
                return BadRequest(new { success = false, message = "Device code is required." });

            var userIdClaim = User.FindFirstValue("userId");
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var success = await _deviceService.LinkDeviceToUserAsync(request.DeviceCode, userId);
            if (!success)
                return BadRequest(new { success = false, message = "Invalid or already used device code." });

            return Ok(new { success = true, message = "Device linked successfully." });
        }
    }

    public record LinkDeviceRequest(string DeviceCode);
}