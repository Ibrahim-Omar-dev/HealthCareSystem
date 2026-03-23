using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Domain.Entities.Identity
{
    public class ResetPasswordRequest
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
