using HealthCare.Domain.Interface;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Infreastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Repository
{
    public class DeviceService : IDeviceService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DeviceService(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Guid?> GetDeviceIdByCodeAsync(string deviceCode)
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.DeviceCode == deviceCode
                                       && d.IsActive == false);
            return device?.Id;
        }

        public async Task<bool> LinkDeviceToUserAsync(string deviceCode, Guid userId)
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.DeviceCode == deviceCode
                                       && d.IsActive == false);
            if (device == null)
                return false;

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return false;

            user.DeviceId = device.Id;
            await _userManager.UpdateAsync(user);

            device.UserId = userId;
            device.IsActive = true;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}