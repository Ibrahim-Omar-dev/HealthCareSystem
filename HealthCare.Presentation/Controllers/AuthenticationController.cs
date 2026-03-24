using HealthCare.Application.Services.Interfaces.IAuthentication;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Interface;
using HealthCare.Domain.User;
using HealthCare.Infreastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ITokenManagement token;


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
            this.token = token;
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

        [HttpGet("google/login")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                                  GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // ─── 2. Google redirects here ──────────────────────────────────────────
        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var frontendBase = _config["Frontend:BaseUrl"]!;

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info is null)
                return Redirect($"{frontendBase}/login?error=google_failed");

            var email = info.Principal.FindFirstValue(ClaimTypes.Email)!;
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email;
            var avatarUrl = info.Principal.FindFirstValue("urn:google:picture");

            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    DisplayName = name,
                    //AvatarUrl = avatarUrl,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return Redirect($"{frontendBase}/login?error={Uri.EscapeDataString(errors)}");
                }
            }
            else
            {
                if (avatarUrl is not null )
                {
                    await _userManager.UpdateAsync(user);
                }
            }

            var existingLogins = await _userManager.GetLoginsAsync(user);

            var alreadyLinked = existingLogins.Any(l =>
                l.LoginProvider == info.LoginProvider &&
                l.ProviderKey == info.ProviderKey);

            if (!alreadyLinked)
                await _userManager.AddLoginAsync(user, info);

            // ✅ create claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email!),
        new Claim(ClaimTypes.Name, user.DisplayName ?? user.Email!)
    };

            // ✅ generate token (no await)
            var gentoken = token.generateToken(claims);

            return Redirect($"{frontendBase}?token={gentoken}");
        }

        // ─── 3. Get current user info ──────────────────────────────────────────
        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Me()
        {
            var identityId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(identityId!);

            if (user is null) return Unauthorized();

            return Ok(new
            {
                user.UserId,
                user.Email,
                user.DisplayName,
               // user.AvatarUrl,
                user.Gender,
                user.BloodType,
                user.DateOfBirth,
                user.PhoneNumber
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
