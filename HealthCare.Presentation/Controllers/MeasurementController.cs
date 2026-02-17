using HealthCare.Domain.Interface;
using HealthCare.Infreastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class MeasurementController : ControllerBase
    {
        private readonly IUserManagement userManagement;
        private readonly AppDbContext context;

        public MeasurementController(IUserManagement userManagement, AppDbContext context)
        {
            this.userManagement = userManagement;
            this.context = context;
        }

        [HttpGet("GetAllData")]
        public async Task<IActionResult> getAllData()
        {
            var SensorMeasurement = await context.Measurements.ToListAsync();
            if (SensorMeasurement == null || !SensorMeasurement.Any())
            {
                return NotFound("No Exit Data");
            }
            return Ok(SensorMeasurement);
        }

        [HttpGet("GetDataByUserId/{userId}")]
        public async Task<IActionResult> getDataByUserId(Guid userId)
        {
            var SensorMeasurement = await context.Measurements
                .Where(_ => _.UserId == userId)
                .ToListAsync(); 

            if (SensorMeasurement == null || !SensorMeasurement.Any())
            {
                return NotFound("No Exit Data");
            }
            return Ok(SensorMeasurement);
        }

        [HttpGet("GetMyData")]
        public async Task<IActionResult> GetMyData()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found in token");
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest("Invalid User ID format");
            }

            var sensorMeasurements = await context.Measurements
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.Id) 
                .ToListAsync();

            if (sensorMeasurements == null || !sensorMeasurements.Any())
            {
                return NotFound("No measurements found for this user");
            }

            return Ok(sensorMeasurements);
        }

        
    }
}