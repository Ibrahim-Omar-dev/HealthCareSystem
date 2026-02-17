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
        
    }
}
