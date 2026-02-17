using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HealthCare.Application.Dto.Cart
{
    public class CreateAcheive
    {
        [Required]
        public Guid ProductId { get; set; }
        [Required]
        public int Quentity { get; set; }
        [Required]
        public Guid UserId { get; set; }
    }
}
