using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Infreastructure.Data;
using Microsoft.Extensions.Configuration;

namespace HealthCare.Infreastructure.Services
{
    public class GoogleAuthorization : IGoogleAuthorization
    {
        private readonly AppDbContext context;
        private readonly IConfiguration config;
        private readonly string redirectUri;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string[] scopes = new[] { "openid", "email", "profile" };

        public GoogleAuthorization(AppDbContext context, IConfiguration config)
        {
            this.context = context;
            this.config = config;
            redirectUri = config["Google:RedirectUri"] ?? string.Empty;
            clientId = config["Google:ClientId"] ?? string.Empty;
            clientSecret = config["Google:ClientSecret"] ?? string.Empty;

            if (string.IsNullOrEmpty(redirectUri) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                throw new InvalidOperationException("Google OAuth settings are not configured. Please set Google:ClientId, Google:ClientSecret and Google:RedirectUri in configuration.");
        }

        public string GetAuthorizationUrl()
        {
            return new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                    Scopes = scopes,
                    Prompt = "consent"
                }).CreateAuthorizationCodeRequest(redirectUri).Build().ToString();
                
        }
        public async Task<GoogleUserInfo?> ExchangeCodeForUserInfo(string code)
        {
            var flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                    Scopes = scopes,
                });

            var token = await flow.ExchangeCodeForTokenAsync(
                "user",
                code: code,
                redirectUri: redirectUri,
                CancellationToken.None);

            // persist token information (optional)
            context.Add(new Credential
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresInSeconds = token.ExpiresInSeconds,
                IdToken = token.IdToken,
                UserId = Guid.NewGuid(),
                IssusedUtc = token.IssuedUtc
            });
            await context.SaveChangesAsync();

            if (string.IsNullOrEmpty(token.IdToken))
                return null;

            // validate id_token to extract user info
            var payload = await GoogleJsonWebSignature.ValidateAsync(token.IdToken);
            if (payload == null || string.IsNullOrEmpty(payload.Email))
                return null;

            return new GoogleUserInfo(payload.Email, payload.Name);
        }
    }
}
