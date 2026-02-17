using HealthCare.Domain.Entities.Cart;

namespace HealthCare.Domain.Interface.Cart
{
    public interface ICart
    {
        Task<int> SaveCheckoutHistory(IEnumerable<Achieve> checkour);
    }
}
