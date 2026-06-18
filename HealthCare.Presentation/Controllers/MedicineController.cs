using HealthCare.Application.Dto;
using HealthCare.Application.Interfaces;
using HealthCare.Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicineController : BaseApiController
    {
        private readonly IMedicineService _medicineService;

        public MedicineController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] AddMedicineDto dto)
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { success = false, message = "Medicine name is required." });

            if (string.IsNullOrWhiteSpace(dto.Dosage))
                return BadRequest(new { success = false, message = "Dosage is required." });

            if (!dto.ReminderTimes.Any())
                return BadRequest(new { success = false, message = "At least one reminder time is required." });

            var result = await _medicineService.AddMedicineAsync(userId, dto);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("GetActive")]
        public async Task<IActionResult> GetActive()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var medicines = await _medicineService.GetMyMedicinesAsync(userId);
            return Ok(medicines);
        }

        [HttpGet("GetCompleted")]
        public async Task<IActionResult> GetCompleted()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var medicines = await _medicineService.GetCompletedMedicinesAsync(userId);
            return Ok(medicines);
        }

        [HttpGet("Get/{medicineId:guid}")]
        public async Task<IActionResult> GetById(Guid medicineId)
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var medicine = await _medicineService.GetMedicineByIdAsync(userId, medicineId);

            return medicine is null
                ? NotFound(new { success = false, message = "Medicine not found." })
                : Ok(medicine);
        }

        [HttpPut("Update/{medicineId:guid}")]
        public async Task<IActionResult> Update(Guid medicineId, [FromBody] AddMedicineDto dto)
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var (success, message) = await _medicineService.UpdateMedicineAsync(userId, medicineId, dto);

            return success
                ? Ok(new { success = true, message })
                : NotFound(new { success = false, message });
        }

        [HttpDelete("Delete/{medicineId:guid}")]
        public async Task<IActionResult> Delete(Guid medicineId)
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var (success, message) = await _medicineService.DeleteMedicineAsync(userId, medicineId);

            return success
                ? Ok(new { success = true, message })
                : NotFound(new { success = false, message });
        }

        [HttpPut("MarkCompleted/{medicineId:guid}")]
        public async Task<IActionResult> MarkCompleted(Guid medicineId)
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var (success, message) = await _medicineService.MarkAsCompletedAsync(userId, medicineId);

            return success
                ? Ok(new { success = true, message })
                : NotFound(new { success = false, message });
        }

        [HttpGet("GetDueReminders")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDueReminders()
        {
            var reminders = await _medicineService.GetDueRemindersAsync();
            return Ok(reminders);
        }
        
    }
}