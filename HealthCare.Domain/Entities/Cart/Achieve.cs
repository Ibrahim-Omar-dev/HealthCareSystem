using System.ComponentModel.DataAnnotations;

namespace HealthCare.Domain.Entities.Cart
{
    public class Achieve
    {
        [Key]
        public Guid Id { get; set; }= Guid.NewGuid();
        public Guid ProductId { get; set; }
        public int Quentity { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedData { get; set; } = DateTime.Now;

    }
}
