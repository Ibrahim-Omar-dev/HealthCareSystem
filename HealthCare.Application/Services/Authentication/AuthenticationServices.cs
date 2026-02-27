using AutoMapper;
using HealthCare.Application.Dto;
using HealthCare.Application.Services.Interfaces.IAuthentication;
using HealthCare.Application.Validation.Services;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Interface;
using HealthCare.Domain.User;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using HealthCare.Domain.Enums;


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

        public AuthenticationServices(IValidationServices validationServices, IValidator<CreateUser> createValidator,
            IValidator<LoginUser> loginValidator,ITokenManagement tokenManagement,
            UserManager<AppUser> userManager,
            IMapper mapper, IUserManagement userManagement, IRoleManagement roleManagement, ILogger<AuthenticationServices>  logger)
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

            var user = await userManagement.GetUserByEmail(createUser.Email);
            if (user == null)
            {
                logger.LogWarning("User {UserName} not found after creation", createUser.UserName);
                return false;
            }

            var allUsers = await userManagement.GetAllUser();
            var role = allUsers.Count() > 1 ? Role.User : Role.Admin;

            var assignRoleResult = await roleManagement.AddUserRole(user, role);
            if (!assignRoleResult)
            {
                logger.LogWarning("Failed to assign role to user {UserName} — rolling back", createUser.UserName);
                await userManagement.DeleteUserByEmail(createUser.Email);
                return false;
            }

            return true;
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

        public async Task<string?> GeneratePasswordResetToken(string email)
        {
            var user = await userManagement.GetUserByEmail(email);
            if (user == null)
            {
                logger.LogWarning("Password reset requested for non-existing email: {Email}", email);
                return null;
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            return token;
        }

        public async Task<bool> ResetPassword(ResetPassword resetPassword)
        {
            var user = await userManagement.GetUserByEmail(resetPassword.Email);
            if (user == null)
            {
                logger.LogWarning("Password reset attempted for non-existing email: {Email}", resetPassword.Email);
                return false;
            }

            var result = await userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.NewPassword);
            if (!result.Succeeded)
            {
                logger.LogWarning("Password reset failed for {Email}: {Errors}", resetPassword.Email, string.Join(";", result.Errors.Select(e => e.Description)));
                return false;
            }

            logger.LogInformation("Password reset successful for {Email}", resetPassword.Email);
            return true;
        }
    }

}