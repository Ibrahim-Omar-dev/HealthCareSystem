using AutoMapper;
using HealthCare.Application.Dto.Cart;
using HealthCare.Domain.Entities.Cart;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.User;

namespace HealthCare.Application.Mapping
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {


            CreateMap<AppUser, CreateUser>().ReverseMap();
            CreateMap<AppUser, LoginUser>().ReverseMap();

            CreateMap<GetPaymentMethod, PaymentMethod>().ReverseMap();

            CreateMap<Achieve, CreateAcheive>().ReverseMap();
        }
    }
}
