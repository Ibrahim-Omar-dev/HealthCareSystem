using System.ComponentModel.DataAnnotations;

namespace HealthCare.Application.Dto.Cart
{
    public class Checkout
    {
        [Required]
        public Guid PaymentMethodId { get; set; }
        [Required]
        public IEnumerable<ProcessCart> Carts{ get; set; }
    }
}
