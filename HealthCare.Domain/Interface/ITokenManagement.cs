using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace HealthCare.Domain.Interface
{
    public interface ITokenManagement
    {
        string GetRefreshToken();
        List<Claim> GetUserClaimsFromToken(string token);
        Task<bool> ValidateRefreshToken(string token);
        Task<string> GetUserIdRefreshToken(string token);
        Task<bool>AddRefreshToken(string userId, string refreshToken);
        Task<bool>UpdateRefreshToken(string userId, string refreshToken);
        string generateToken(IEnumerable<Claim> claim);

    }
}
