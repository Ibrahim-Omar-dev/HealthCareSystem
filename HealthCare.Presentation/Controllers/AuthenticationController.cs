using HealthCare.Application.Services.Interfaces.IAuthentication;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.User;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationServices authenticationService;
        private readonly HealthCare.Infreastructure.Services.IGoogleAuthorization googleAuthorization;

        public AuthenticationController(IAuthenticationServices authenticationService, HealthCare.Infreastructure.Services.IGoogleAuthorization googleAuthorization)
        {
            this.authenticationService = authenticationService;
            this.googleAuthorization = googleAuthorization;
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
        // POST api/Authentication/ForgotPassword
        // Body: { "email": "user@example.com" }
        //[HttpPost("ForgotPassword")]
        //public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        //{
        //    if (string.IsNullOrWhiteSpace(request.Email))
        //        return BadRequest(new { success = false, message = "Email is required." });

        //    await authenticationService.ForgotPasswordAsync(request.Email);

        //    // Always return OK to prevent email enumeration
        //    return Ok(new { success = true, message = "If that email is registered, a reset link has been sent." });
        //}

        //// POST api/Authentication/SendLoginCode
        //// Body: { "email":"user@example.com" }
        //[HttpPost("SendLoginCode")]
        //public async Task<IActionResult> SendLoginCode([FromBody] ForgotPasswordRequest request)
        //{
        //    if (string.IsNullOrWhiteSpace(request.Email))
        //        return BadRequest(new { success = false, message = "Email is required." });

        //    var ok = await authenticationService.SendLoginCodeAsync(request.Email);
        //    if (!ok)
        //        return StatusCode(500, new { success = false, message = "Failed to send login code." });

        //    return Ok(new { success = true, message = "If the email exists, a login code has been sent." });
        //}

        //// POST api/Authentication/LoginWithCode
        //// Body: { "email":"user@example.com", "code":"123456" }
        //[HttpPost("LoginWithCode")]
        //public async Task<IActionResult> LoginWithCode([FromBody] HealthCare.Domain.Entities.Identity.LoginWithCodeRequest request)
        //{
        //    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
        //        return BadRequest(new { success = false, message = "Email and code are required." });

        //    var result = await authenticationService.LoginWithCodeAsync(request.Email, request.Code);

        //    if (result.Issucess)
        //    {
        //        return Ok(new
        //        {
        //            isSuccess = result.Issucess,
        //            message = result.Message,
        //            token = result.Token,
        //            refreshToken = result.RefreshToken
        //        });
        //    }

        //    return BadRequest(new { isSuccess = result.Issucess, message = result.Message });
        //}

        //// POST api/Authentication/ResetPassword
        //// Body: { "token": "abc123...", "newPassword": "MyNewPass123!" }
        //[HttpPost("ResetPassword")]
        //public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        //{
        //    if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        //        return BadRequest(new { success = false, message = "Token and new password are required." });

        //    var (isSuccess, message) = await authenticationService.ResetPasswordAsync(request.Token, request.NewPassword);

        //    return isSuccess
        //        ? Ok(new { success = true, message })
        //        : BadRequest(new { success = false, message });
        //}

        //// POST api/Authentication/LoginWithGoogle
        //// Body: { "code": "authorization_code_from_google" }
        //[HttpPost("LoginWithGoogle")]
        //public async Task<IActionResult> LoginWithGoogle([FromBody] string code)
        //{
        //    if (string.IsNullOrWhiteSpace(code))
        //        return BadRequest(new { success = false, message = "Authorization code is required." });

        //    var userInfo = await googleAuthorization.ExchangeCodeForUserInfo(code);
        //    if (userInfo == null)
        //        return BadRequest(new { success = false, message = "Failed to exchange code for user info." });

        //    var result = await authenticationService.ExternalLogin(userInfo.Email, userInfo.Name);

        //    if (result.Issucess)
        //    {
        //        return Ok(new
        //        {
        //            isSuccess = result.Issucess,
        //            message = result.Message,
        //            token = result.Token,
        //            refreshToken = result.RefreshToken
        //        });
        //    }

        //    return BadRequest(new { isSuccess = result.Issucess, message = result.Message });
        //}
    }
}
