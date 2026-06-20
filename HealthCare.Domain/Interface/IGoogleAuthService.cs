using HealthCare.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Domain.Interface
{
    public interface IGoogleAuthService
    {
        Task<(bool Success, AppUser? User, string Message)> LoginOrRegisterAsync(string idToken);
    }
}
