using HealthCare.Application.Dto;
using HealthCare.Application.Dto.Cart;

namespace HealthCare.Application.Services.Interfaces.Cart
{
    public interface ICartServices
    {
        Task<ServicesResponse> SaveCheckoutHistory(IEnumerable<CreateAcheive> createCarts);
        Task<ServicesResponse> Checkout(Checkout checkout, string userId);
    }
}
