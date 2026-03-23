using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Interface;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HealthCare.Infreastructure.Repository.Authentication
{
    public class PasswordResetRepository 
    {
        private readonly AppDbContext context;

        public PasswordResetRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(PasswordResetCode code)
        {
            context.PasswordResetCodes.Add(code);
            await context.SaveChangesAsync();
        }

        public async Task<PasswordResetCode?> GetByUserIdAndCode(string userId, string code)
        {
            return await context.PasswordResetCodes.FirstOrDefaultAsync(r => r.UserId == userId && r.Code == code);
        }

        public async Task RemoveAsync(PasswordResetCode code)
        {
            context.PasswordResetCodes.Remove(code);
            await context.SaveChangesAsync();
        }
    }
}
