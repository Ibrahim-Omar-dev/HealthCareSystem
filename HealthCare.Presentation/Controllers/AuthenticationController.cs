using HealthCare.Application.Dto.RestPasswordDTo;
using HealthCare.Application.Dto.User;
using HealthCare.Application.Services.Interfaces.IAuthentication;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Interface;
using HealthCare.Domain.User;
using HealthCare.Infreastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : BaseApiController
    {
        private readonly IAuthenticationServices authenticationService;
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;
        private readonly IUserManagement userManagement;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenManagement jwttoken;
        private readonly IGoogleAuthService _googleAuthService;




        public AuthenticationController(IAuthenticationServices authenticationService, IConfiguration config,
                AppDbContext db, IUserManagement userManagement, SignInManager<AppUser> signInManager
                , ITokenManagement token, IGoogleAuthService googleAuthService
           )
        {
            this.authenticationService = authenticationService;
            _config = config;
            _db = db;
            this.userManagement = userManagement;
            _signInManager = signInManager;
            this.jwttoken = token;
            _googleAuthService = googleAuthService;
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
            var result = await authenticationService.ReviveToken(refreshToken);
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
        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
                return BadRequest(new { success = false, message = "Google ID token is required." });

            var (success, user, message) = await _googleAuthService.LoginOrRegisterAsync(request.IdToken);

            if (!success || user is null)
                return BadRequest(new { success = false, message });

            var claims = await userManagement.GetUserClaims(user.Email!);


            return Ok(new
            {
                success = true,
                message,
                userId = user.Id
            });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await authenticationService.ForgotPasswordAsync(request.Email);
            return Ok(new { isSuccess = result.IsSuccess, message = result.Message });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await authenticationService.VerifyOtpAsync(request.Email, request.Otp);

            return result.IsSuccess
                ? Ok(new { isSuccess = true, message = result.Message })
                : BadRequest(new { isSuccess = false, message = result.Message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await authenticationService.ResetPasswordAsync(
                             request.Email, request.Otp, request.NewPassword);

            return result.IsSuccess
                ? Ok(new { isSuccess = true, message = result.Message })
                : BadRequest(new { isSuccess = false, message = result.Message });
        }

    }
}
