using HealthCare.Application.Dto.Cart;

namespace HealthCare.Application.Services.Interfaces.Cart
{
    public interface IPaymentMethodServices
    {
        Task<IEnumerable<GetPaymentMethod>> GetPaymentMethods();
    }
}
