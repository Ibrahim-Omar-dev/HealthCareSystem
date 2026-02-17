using AutoMapper;
using HealthCare.Application.Dto;
using HealthCare.Application.Dto.Cart;
using HealthCare.Application.Services.Interfaces.Cart;
using HealthCare.Domain.Entities.Cart;
using HealthCare.Domain.Interface.Cart;

namespace HealthCare.Application.Services.Cart
{
    public class CartServices : ICartServices
    {
        private readonly ICart cart;
        private readonly IMapper mapper;

        public CartServices(ICart cart,IMapper mapper)
        {
            this.cart = cart;
            this.mapper = mapper;
        }

        public Task<ServicesResponse> Checkout(Checkout checkout, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ServicesResponse> SaveCheckoutHistory(IEnumerable<CreateAcheive> createCarts)
        {
            var mappedData = mapper.Map<IEnumerable<Achieve>>(createCarts);
            var result = await cart.SaveCheckoutHistory(mappedData);
            return result > 0 ? new ServicesResponse { IsSuccess = true, Message = "Checkout Successful" } :
                new ServicesResponse { IsSuccess = false, Message = "Error occur in saving" };
        }

    }
}
