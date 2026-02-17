using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Interface;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HealthCare.Infreastructure.Repository.Authentication
{
    internal class TokenManagement : ITokenManagement
    {
        private readonly AppDbContext context;
        private readonly IConfiguration config;

        public TokenManagement(AppDbContext context,IConfiguration config )
        {
            this.context = context;
            this.config = config;
        }
        public async Task<bool> AddRefreshToken(string userId, string refreshToken)
        {
            context.RefreshTokens.Add(new RefreshToken
            {
                UserId= userId,
                Token = refreshToken
            });
            await context.SaveChangesAsync();
            return true;
        }

        public string generateToken(IEnumerable<Claim> claim)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF32.GetBytes(config["JwtSettings:SecretKey"]!));
            var cred=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
            var expiration=DateTime.Now.AddHours(2);
            var token = new JwtSecurityToken(
                issuer: config["JwtSettings:Issuer"],
                audience: config["JwtSettings:Audience"],
                claims: claim,
                expires: expiration,
                signingCredentials: cred
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GetRefreshToken()
        {
            const int byteSize = 64;
            byte[] randomByte=new byte[byteSize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomByte);
            }
            string token= Convert.ToBase64String(randomByte);
            return WebUtility.UrlEncode(token);
        }

        public List<Claim> GetUserClaimsFromToken(string token)
        {
            var tokenHandler=new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            if (jwtToken != null)
                return jwtToken.Claims.ToList();
            return [];
        }

        public async Task<string> GetUserIdRefreshToken(string token)
        {
            var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
            return refreshToken!.UserId;
        }

        public async Task<bool> UpdateRefreshToken(string userId, string newRefreshToken)
        {
            var existingToken = await context.RefreshTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (existingToken == null)
                return false;

            existingToken.Token = newRefreshToken;
            existingToken.ExpireDate = DateTime.UtcNow.AddDays(7);
            existingToken.CreateDate = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateRefreshToken(string token)
        {
            var user=await context.RefreshTokens.FirstOrDefaultAsync(t=>t.Token == token);
            return user != null;
        }
    }
}
