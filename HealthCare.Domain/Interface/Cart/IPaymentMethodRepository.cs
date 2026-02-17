using HealthCare.Domain.Entities.Cart;

namespace HealthCare.Domain.Interface.Cart
{
    public interface IPaymentMethodRepository
    {
        Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync();
    }
}
