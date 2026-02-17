using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HealthCare.Domain.Entities.Cart
{
    public class PaymentMethod
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        [Required]
        public string Name { get; set; }=string.Empty;
    }
}
