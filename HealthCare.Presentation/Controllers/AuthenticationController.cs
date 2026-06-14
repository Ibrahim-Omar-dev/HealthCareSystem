using Google.Apis.Auth;
using HealthCare.Application.Dto.RestPasswordDTo;
using HealthCare.Application.Services.Interfaces.IAuthentication;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Interface;
using HealthCare.Domain.User;
using HealthCare.Infreastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationServices authenticationService;
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenManagement jwttoken;


        public AuthenticationController(IAuthenticationServices authenticationService, IConfiguration config,
                AppDbContext db, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager
                , ITokenManagement token
           )
        {
            this.authenticationService = authenticationService;
            _config = config;
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            this.jwttoken = token;
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
        [HttpPost("google")]
        public async Task<IActionResult> GoogleMobileLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                // 1. Verify IdToken with Google
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    request.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _config["Google:ClientId"] },
                        ExpirationTimeClockTolerance = TimeSpan.FromMinutes(5)
                    });

                // 2. Find or create user
                var user = await _userManager.FindByEmailAsync(payload.Email);

                if (user is null)
                {
                    user = new AppUser
                    {
                        UserName = payload.Email,
                        Email = payload.Email,
                        DisplayName = payload.Name,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                        return BadRequest(new
                        {
                            message = "Failed to create user",
                            errors = result.Errors.Select(e => e.Description)
                        });

                    await _userManager.AddLoginAsync(user, new UserLoginInfo(
                        "Google", payload.Subject, "Google"));
                }

                // 3. Build claims
                var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email,          user.Email          ?? ""),
            new(ClaimTypes.Name,           user.DisplayName    ?? ""),
            new("userId",                  user.Id.ToString()),
            new("gender",                  user.Gender?.ToString()    ?? ""),
            new("bloodType",               user.BloodType?.ToString() ?? ""),
            new("dateOfBirth",             user.DateOfBirth?.ToString("yyyy-MM-dd") ?? ""),
        };

                // 4. Add roles to claims
                var roles = await _userManager.GetRolesAsync(user);
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                // 5. ✅ Generate the token string
                var token = jwttoken.generateToken(claims);

                // 6. ✅ Return the token string not the service
                return Ok(new
                {
                    token,
                    user = new
                    {
                        user.Id,
                        user.Email,
                        user.DisplayName,
                        user.Gender,
                        user.BloodType,
                        user.DateOfBirth
                    }
                });
            }
            catch (InvalidJwtException ex)
            {
                return Unauthorized(new
                {
                    message = "Invalid Google token",
                    reason = ex.Message,
                    clientId = _config["Google:ClientId"]
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong", error = ex.Message });
            }
        }

        public class GoogleLoginRequest
        {
            [Required]
            public string IdToken { get; set; } = "";
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
