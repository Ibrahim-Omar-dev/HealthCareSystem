using HealthCare.Domain.Entities.Identity;

namespace HealthCare.Domain.Interface
{
    public interface IPasswordResetRepository
    {
        Task SaveResetTokenAsync(string userId, string token, DateTime expiry);
        Task<(string UserId, DateTime Expiry)?> GetResetTokenAsync(string token);
        Task DeleteResetTokenAsync(string token);
    }
}
