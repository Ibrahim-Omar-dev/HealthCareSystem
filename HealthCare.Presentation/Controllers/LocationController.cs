using HealthCare.Application.Dto.LocationDTOS;
using HealthCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
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
            public async Task<IActionResult> Get()
            {
                if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                    return error!;

                var location = await _locationService.GetMyLocationAsync(userId);

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
