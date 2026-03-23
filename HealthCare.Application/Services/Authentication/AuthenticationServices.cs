
using AutoMapper;
using FluentValidation;
using HealthCare.Application.Services.Interfaces;
using HealthCare.Application.Services.Interfaces.IAuthentication;
using HealthCare.Application.Validation.Services;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Interface;
using HealthCare.Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    private readonly IPasswordResetRepository passwordResetRepository;
    private readonly IEmailService emailService;

        public AuthenticationServices(IValidationServices validationServices, IValidator<CreateUser> createValidator,
            IValidator<LoginUser> loginValidator, ITokenManagement tokenManagement,
            UserManager<AppUser> userManager,
            IMapper mapper, IUserManagement userManagement, IRoleManagement roleManagement, ILogger<AuthenticationServices> logger,
            IPasswordResetRepository passwordResetRepository,
            IEmailService emailService)
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
            this.passwordResetRepository = passwordResetRepository;
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

            var saveTokenResult = await tokenManagement.AddRefreshToken(_user.Id, refreshToken);
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

            var saveTokenResult = await tokenManagement.AddRefreshToken(_user.Id, refreshToken);
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
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await userManagement.GetUserByEmail(email);

                // Always return true to prevent email enumeration
                if (user == null)
                {
                    logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                    return true;
                }

                // Generate secure token
                var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
                var expiry = DateTime.UtcNow.AddHours(1);

                await passwordResetRepository.SaveResetTokenAsync(user.Id, token, expiry);

   
                var resetLink = $"https://yourfrontend.com/reset-password?token={token}";

                await emailService.SendPasswordResetEmailAsync(email, resetLink);

                logger.LogInformation("Password reset email sent to {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending password reset email to {Email}", email);
                return false;
            }
        }

        public async Task<(bool IsSuccess, string Message)> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                var record = await passwordResetRepository.GetResetTokenAsync(token);

                if (record == null)
                    return (false, "Invalid or expired reset token.");

                if (record.Value.Expiry < DateTime.UtcNow)
                {
                    await passwordResetRepository.DeleteResetTokenAsync(token);
                    return (false, "Reset token has expired. Please request a new one.");
                }

                var user = await userManagement.GetUserById(record.Value.UserId);
                if (user == null)
                    return (false, "User not found.");

                // Reset password using Identity (handles hashing automatically)
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, resetToken, newPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);
                    return (false, $"Password reset failed: {errors}");
                }

                // Invalidate the token after successful reset
                await passwordResetRepository.DeleteResetTokenAsync(token);

                logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
                return (true, "Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error resetting password with token {Token}", token);
                return (false, "An error occurred while resetting the password.");
            }
        }

        public async Task<bool> SendLoginCodeAsync(string email)
        {
            try
            {
                var user = await userManagement.GetUserByEmail(email);
                // don't reveal whether the email exists
                if (user == null)
                {
                    logger.LogWarning("Login code requested for non-existent email: {Email}", email);
                    return true;
                }

                var rng = new Random();
                var code = rng.Next(100000, 999999).ToString();
                var expiry = DateTime.UtcNow.AddMinutes(10);

                await passwordResetRepository.SaveResetTokenAsync(user.Id, code, expiry);

                var subject = "Your login code";
                var body = $"Your login code is: {code}. It will expire in 10 minutes.";
                await emailService.SendEmailAsync(email, subject, body);

                logger.LogInformation("Login code sent to {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending login code to {Email}", email);
                return false;
            }
        }

        public async Task<LoginResponse> LoginWithCodeAsync(string email, string code)
        {
            try
            {
                var user = await userManagement.GetUserByEmail(email);
                if (user == null)
                    return new LoginResponse { Issucess = false, Message = "Invalid code or email" };

                var record = await passwordResetRepository.GetResetTokenAsync(code);
                if (record == null || record.Value.UserId != user.Id)
                    return new LoginResponse { Issucess = false, Message = "Invalid code or email" };

                if (record.Value.Expiry < DateTime.UtcNow)
                {
                    await passwordResetRepository.DeleteResetTokenAsync(code);
                    return new LoginResponse { Issucess = false, Message = "Code expired" };
                }

                var claims = await userManagement.GetUserClaims(user.Email!);
                string jwtToken = tokenManagement.generateToken(claims);
                var refreshToken = tokenManagement.GetRefreshToken();

                var saveTokenResult = await tokenManagement.AddRefreshToken(user.Id, refreshToken);
                if (!saveTokenResult)
                    return new LoginResponse { Issucess = false, Message = "Internal error occurred while authentication" };

                await passwordResetRepository.DeleteResetTokenAsync(code);

                return new LoginResponse
                {
                    Issucess = true,
                    Message = "Successful Login",
                    Token = jwtToken,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during login with code for {Email}", email);
                return new LoginResponse { Issucess = false, Message = "An error occurred" };
            }
        }

    }
}