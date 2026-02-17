using HealthCare.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Domain.Interface
{
    public interface IRoleManagement
    {
        Task<string?> GetUserRole(string email);
        Task<bool> AddUserRole(AppUser user,string roleName);
    }
}
