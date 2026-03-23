using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Domain.Entities.Identity
{
    public class Credential
    {
        public Guid UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public long? ExpiresInSeconds { get; set; }
        public string IdToken { get; set; }
        public DateTime IssusedUtc { get; set; }
    }
}
