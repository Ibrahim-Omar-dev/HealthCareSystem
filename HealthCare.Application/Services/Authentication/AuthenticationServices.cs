
using AutoMapper;
using FluentValidation;
using HealthCare.Application.Services.Interfaces;
using HealthCare.Application.Services.Interfaces.IAuthentication;
using HealthCare.Application.Validation.Services;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Interface;
using HealthCare.Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Services.Authentication
{
    public class AuthenticationServices : IAuthenticationServices
    {
        private readonly IValidationServices validationServices;
        private readonly IValidator<LoginUser> loginValidator;
        private readonly ITokenManagement tokenManagement;
        private readonly UserManager<AppUser> userManager;
        private readonly IValidator<CreateUser> createValidator;
        private readonly IMapper mapper;
        private readonly IUserManagement userManagement;
    private readonly IRoleManagement roleManagement;
    private readonly ILogger<AuthenticationServices> logger;
        private readonly IConfiguration configuration;
        private readonly IEmailService emailService;

        public AuthenticationServices(IValidationServices validationServices, IValidator<CreateUser> createValidator,
            IValidator<LoginUser> loginValidator, ITokenManagement tokenManagement,
            UserManager<AppUser> userManager,
            IMapper mapper, IUserManagement userManagement, IRoleManagement roleManagement, ILogger<AuthenticationServices> logger
            ,IConfiguration configuration, IEmailService emailService
            )
        {
            this.validationServices = validationServices;
            this.loginValidator = loginValidator;
            this.tokenManagement = tokenManagement;
            this.userManager = userManager;
            this.createValidator = createValidator;
            this.mapper = mapper;
            this.userManagement = userManagement;
            this.roleManagement = roleManagement;
            this.logger = logger;
            this.configuration = configuration;
            this.emailService = emailService;
        }

        public async Task<LoginResponse> ExternalLogin(string email, string? userName)
        {
            var _user = await userManagement.GetUserByEmail(email);
            if (_user == null)
            {
                // create new user with random password
                var pwd = Guid.NewGuid().ToString("N").Substring(0, 12) + "aA1!";
                var createUser = new CreateUser
                {
                    Email = email,
                    UserName = string.IsNullOrEmpty(userName) ? email : userName,
                    Password = pwd,
                    ConfirmPassword = pwd
                };

                var created = await userManagement.CreateUser(createUser);
                if (!created)
                    return new LoginResponse { Issucess = false, Message = "Failed to create user for external login" };

                var createdUser = await userManagement.GetUserByEmail(email);
                if (createdUser == null)
                    return new LoginResponse { Issucess = false, Message = "Failed to retrieve newly created user" };

                var users = await userManagement.GetAllUser();
                bool assignRoleResult = await roleManagement.AddUserRole(createdUser, users.Count() > 1 ? "user" : "admin");
                if (!assignRoleResult)
                    return new LoginResponse { Issucess = false, Message = "Failed to assign role to external user" };

                _user = createdUser;
            }

            string? roleName = await roleManagement.GetUserRole(_user.Email!);
            if (string.IsNullOrEmpty(roleName))
            {
                return new LoginResponse { Issucess = false, Message = "User role not assigned" };
            }

            var claims = await userManagement.GetUserClaims(_user.Email!);
            string jwtToken = tokenManagement.generateToken(claims);
            var refreshToken = tokenManagement.GetRefreshToken();

            var saveTokenResult = await tokenManagement.AddRefreshToken(_user.Id.ToString(), refreshToken);
            if (!saveTokenResult)
                return new LoginResponse { Issucess = false, Message = "Internal error occurred while authentication" };

            return new LoginResponse
            {
                Issucess = true,
                Message = "Successful Login",
                Token = jwtToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<bool> CreateUser(CreateUser createUser)
        {
            var validationResult = await validationServices.ValidateAsync(createUser, createValidator);
            if (!validationResult.IsSuccess)
            {
                logger.LogWarning("User creation validation failed: {Message}", validationResult.Message);
                return false;
            }

            var result = await userManagement.CreateUser(createUser);
            if (!result)
            {
                logger.LogWarning("Failed to create user {UserName}", createUser.UserName);
                return false;
            }

            logger.LogInformation("User {UserName} created successfully", createUser.UserName);

            var _user = await userManagement.GetUserByEmail(createUser.Email);
            if (_user == null)
            {
                logger.LogWarning("User {UserName} not found after creation", createUser.UserName);
                return false;
            }

            var users = await userManagement.GetAllUser();
            bool assignRoleResult = await roleManagement.AddUserRole(_user, users.Count() > 1 ? "user" : "admin");
            if (!assignRoleResult)
            {
                var removeResult = await userManagement.DeleteUserByEmail(createUser.Email);
                if (!removeResult)
                {
                    logger.LogWarning("Failed to assign role to user {UserName}", createUser.UserName);
                    return false;
                }
            }
            return result;
        }
        public async Task<LoginResponse> Login(LoginUser loginUser)
        {
            var validationResult = await validationServices.ValidateAsync(loginUser, loginValidator);
            if (!validationResult.IsSuccess)
            {
                return new LoginResponse { Issucess = false, Message = validationResult.Message };
            }

            var _user = await userManagement.GetUserByEmail(loginUser.Email);
            if (_user == null)
            {
                return new LoginResponse { Issucess = false, Message = "Email not found or invalid credentials" };
            }

            var passwordValid = await userManager.CheckPasswordAsync(_user, loginUser.Password);
            if (!passwordValid)
            {
                return new LoginResponse { Issucess = false, Message = "Email not found or invalid credentials" };
            }

            string? roleName = await roleManagement.GetUserRole(_user.Email!);
            if (string.IsNullOrEmpty(roleName))
            {
                return new LoginResponse { Issucess = false, Message = "User role not assigned" };
            }

            var claims = await userManagement.GetUserClaims(_user.Email!);
            string jwtToken = tokenManagement.generateToken(claims);
            var refreshToken = tokenManagement.GetRefreshToken();

            var saveTokenResult = await tokenManagement.AddRefreshToken(_user.Id.ToString(), refreshToken);
            if (!saveTokenResult)
                return new LoginResponse { Issucess = false, Message = "Internal error occurred while authentication" };

            return new LoginResponse
            {
                Issucess = true,
                Message = "Successful Login",
                Token = jwtToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<LoginResponse> ReviveToken(string refreshToken)
        {
            var validationTokenResult = await tokenManagement.ValidateRefreshToken(refreshToken);
            if (!validationTokenResult)
                return new LoginResponse { Issucess = false, Message = "Invalid Token" };

            string userId = await tokenManagement.GetUserIdRefreshToken(refreshToken);
            if (string.IsNullOrEmpty(userId))
                return new LoginResponse { Issucess = false, Message = "Invalid Token" };

            AppUser? user = await userManagement.GetUserById(userId);
            if (user == null)
                return new LoginResponse { Issucess = false, Message = "User not found" };

            var claims = await userManagement.GetUserClaims(user.Email!);
            string newJwtToken = tokenManagement.generateToken(claims);
            string newRefreshToken = tokenManagement.GetRefreshToken();

            var updateResult = await tokenManagement.UpdateRefreshToken(userId, newRefreshToken);
            if (!updateResult)
                return new LoginResponse { Issucess = false, Message = "Failed to update refresh token" };

            return new LoginResponse
            {
                Issucess = true,
                Message = "Token refreshed successfully",
                Token = newJwtToken,
                RefreshToken = newRefreshToken
            };
        }
        public async Task<(bool IsSuccess, string Message)> ForgotPasswordAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            // Always return success to avoid user enumeration
            if (user is null)
                return (true, "If that email exists, an OTP has been sent.");

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            await userManager.UpdateAsync(user);

            await emailService.SendOtpEmailAsync(email, otp);

            return (true, "OTP sent to your email. Valid for 10 minutes.");
        }

        public async Task<(bool IsSuccess, string Message)> VerifyOtpAsync(string email, string otp)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user is null || user.OtpCode != otp || user.OtpExpiry < DateTime.UtcNow)
                return (false, "Invalid or expired OTP.");

            return (true, "OTP verified successfully.");
        }

        public async Task<(bool IsSuccess, string Message)> ResetPasswordAsync(
            string email, string otp, string newPassword)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user is null || user.OtpCode != otp || user.OtpExpiry < DateTime.UtcNow)
                return (false, "Invalid or expired OTP.");

            // Reset password using Identity token
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            // Clear OTP after successful reset
            user.OtpCode = null;
            user.OtpExpiry = null;
            await userManager.UpdateAsync(user);

            return (true, "Password reset successfully.");
        }
    }
}