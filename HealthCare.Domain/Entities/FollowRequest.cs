using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Entities
{
    public class FollowRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SenderId { get; set; }

        public Guid ReceiverId { get; set; }

        public FollowRequestStatus Status { get; set; } = FollowRequestStatus.Pending;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }

        [ForeignKey("SenderId")]
        public AppUser? Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public AppUser? Receiver { get; set; }
    }
}