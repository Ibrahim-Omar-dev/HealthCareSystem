using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Domain.Entities.Identity
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpireDate { get; set; } = DateTime.UtcNow;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    }
}
