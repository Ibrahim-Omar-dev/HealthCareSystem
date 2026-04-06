using AutoMapper;
using HealthCare.Domain.Entities.Profile;
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
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IMapper mapper;

        public ProfileController(AppDbContext context,IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        [HttpGet("GetUserData")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
                return NotFound("User not found");

            var userDto = mapper.Map<EditUserData>(user);
            return Ok(userDto);
        }
        [HttpPost("UpdateUserProfile")]
        
        public async Task<IActionResult> UpdateUserProfile(EditUserData editUserData)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
                return NotFound("User not found");

            user.DisplayName = editUserData.DisplayName;
            user.PhoneNumber = editUserData.PhoneNumber;
            user.DateOfBirth = editUserData.BirthDate;
            user.Gender = editUserData.Gender;
            user.BloodType = editUserData.BloodType;

            await context.SaveChangesAsync();

            return Ok("Profile updated successfully");
        }
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized("Invalid token");

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
                return NotFound("User not found");

            var refreshTokens = context.RefreshTokens
                .Where(r => r.UserId == userId);

            context.RefreshTokens.RemoveRange(refreshTokens);

            context.Users.Remove(user);

            await context.SaveChangesAsync();

            return Ok("Account deleted successfully");
        }
    }
}
