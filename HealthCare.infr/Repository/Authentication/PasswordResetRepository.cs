using HealthCare.Domain.Entities;
using HealthCare.Domain.Interface;
using HealthCare.Domain.User;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Repositories
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly AppDbContext _context;

        public PasswordResetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveResetTokenAsync(string userId, string token, DateTime expiry)
        {
            // Remove any existing token for this user
            var existing = await _context.PasswordResetTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (existing.Any())
                _context.PasswordResetTokens.RemoveRange(existing);

            await _context.PasswordResetTokens.AddAsync(new PasswordResetToken
            {
                UserId = userId,
                Token = token,
                Expiry = expiry
            });

            await _context.SaveChangesAsync();
        }

        public async Task<(string UserId, DateTime Expiry)?> GetResetTokenAsync(string token)
        {
            var record = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (record == null) return null;

            return (record.UserId, record.Expiry);
        }

        public async Task DeleteResetTokenAsync(string token)
        {
            var record = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (record != null)
            {
                _context.PasswordResetTokens.Remove(record);
                await _context.SaveChangesAsync();
            }
        }
    }
}