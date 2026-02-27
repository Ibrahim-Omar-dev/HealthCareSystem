using HealthCare.Application.Services.Interfaces.IAuthentication;
using HealthCare.Domain.User;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationServices authenticationService;

        public AuthenticationController(IAuthenticationServices authenticationService)
        {
            this.authenticationService = authenticationService;
        }
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser(CreateUser createUser)
        {
            var result = await authenticationService.CreateUser(createUser);
            return result
                ? Ok(new { success = true, message = "User created successfully" })
                : BadRequest(new { success = false, message = "User already exists or creation failed" });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginUser loginUser)
        {
            var result = await authenticationService.Login(loginUser);

            if (result.Issucess)
            {
                return Ok(new
                {
                    isSuccess = result.Issucess,
                    message = result.Message,
                    token = result.Token,
                    refreshToken = result.RefreshToken
                });
            }

            return BadRequest(new
            {
                isSuccess = result.Issucess,
                message = result.Message
            });
        }
        [HttpGet("RefreshToken/{refreshToken}")]
        public async Task<IActionResult> ReviveToken(string refreshToken)
        {
            var result=await authenticationService.ReviveToken(refreshToken);
            if (result.Issucess)
            {
                return Ok(new
                {
                    isSuccess = result.Issucess,
                    message = result.Message,
                    token = result.Token,
                    refreshToken = result.RefreshToken
                });
            }

            return BadRequest(new
            {
                isSuccess = result.Issucess,
                message = result.Message
            });
        }


        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { success = false, message = "Email is required" });

            var token = await authenticationService.GeneratePasswordResetToken(email);
            if (token == null)
                return BadRequest(new { success = false, message = "Email not found" });

     
            return Ok(new { success = true, message = "Password reset token generated", token });
        }

        // Reset password using the token received via email.
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword resetPassword)
        {
            if (resetPassword == null)
                return BadRequest(new { success = false, message = "Invalid request" });

            var result = await authenticationService.ResetPassword(resetPassword);
            if (!result)
                return BadRequest(new { success = false, message = "Failed to reset password. Token may be invalid or expired." });

            return Ok(new { success = true, message = "Password has been reset successfully" });
        }
        
    }
}
