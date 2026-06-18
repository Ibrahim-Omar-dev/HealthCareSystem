using HealthCare.Application.Dto.LocationDTOS;
using HealthCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : BaseApiController
    {
            private readonly ILocationService _locationService;

            public LocationController(ILocationService locationService)
            {
                _locationService = locationService;
            }


            [HttpPost("Post")]
            public async Task<IActionResult> Post([FromBody] AddLocationDto dto)
            {
                if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                    return error!;

                var (success, message, data) = await _locationService.AddLocationAsync(userId, dto);

                if (!success)
                    return BadRequest(new { success = false, message });

                return Ok(new { success = true, message, data });
            }

            [HttpGet("Get")]
        [AllowAnonymous]
            public async Task<IActionResult> Get()
            {

                var location = await _locationService.GetMyLocationAsync();

                if (location is null)
                    return NotFound(new
                    {
                        success = false,
                        message = "No location found. Please post your coordinates first."
                    });

                return Ok(location);
            }
        }
    }
