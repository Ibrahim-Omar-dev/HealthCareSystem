using Google.Apis.Auth.OAuth2;

namespace HealthCare.Infreastructure.Services
{
    public interface IGoogleAuthServices
    {
        string[] GetScopes();
        string ScopeToString();
        ClientSecrets GetClientSecrets();
    }
}
