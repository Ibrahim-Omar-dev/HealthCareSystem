using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthCare.Presentation.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected bool TryGetCurrentUserId(out Guid userId, out IActionResult? error)
        {
            userId = Guid.Empty;
            error = null;

            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(claim))
            {
                error = Unauthorized(new { success = false, message = "User ID not found in token." });
                return false;
            }

            if (!Guid.TryParse(claim, out userId))
            {
                error = BadRequest(new { success = false, message = "Invalid User ID format." });
                return false;
            }

            return true;
        }
    }
}
