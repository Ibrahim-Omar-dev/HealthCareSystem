using HealthCare.Application.Dto.Accelerometer;
using HealthCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccelerometerController : BaseApiController
    {
        private readonly IAccelerometerService _accelerometerService;

        public AccelerometerController(IAccelerometerService accelerometerService)
        {
            _accelerometerService = accelerometerService;
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

       
    }
}
