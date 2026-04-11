using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Domain.Interface
{
        public interface IDeviceService
        {
            Task<Guid?> GetDeviceIdByCodeAsync(string deviceCode);
            Task<bool> LinkDeviceToUserAsync(string deviceCode, Guid userId);
        }
    
}

