namespace HealthCare.Infreastructure.Services
{
    public record GoogleUserInfo(string Email, string? Name);

    public interface IGoogleAuthorization
    {
        string GetAuthorizationUrl();
        // Exchanges the authorization code and returns basic user info (email + name) or null.
        Task<GoogleUserInfo?> ExchangeCodeForUserInfo(string code);
    }
}
