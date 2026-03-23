using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Microsoft.Extensions.Configuration;

namespace HealthCare.Infreastructure.Services
{
    public class GoogleAuthServices : IGoogleAuthServices
    {
        private readonly IConfiguration configuration;

        public GoogleAuthServices(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public ClientSecrets GetClientSecrets()
        {
            string clientId = configuration["Google:ClientId"];
            string clientSecret = configuration["Google:ClientSecret"];
            return new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };
        }

        public string[] GetScopes()
        {
            var scopes = new string[]
             {
                  Oauth2Service.Scope.Openid,
                  Oauth2Service.Scope.UserinfoEmail,
                  Oauth2Service.Scope.UserinfoProfile
            };

            return scopes;
        }

        public string ScopeToString()=> string.Join(" ", GetScopes());
    }
}
