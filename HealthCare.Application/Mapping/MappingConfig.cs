using AutoMapper;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Entities.Profile;
using HealthCare.Domain.User;

namespace HealthCare.Application.Mapping
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {


            CreateMap<AppUser, CreateUser>().ReverseMap();
            CreateMap<AppUser, LoginUser>().ReverseMap();
            CreateMap<AppUser, EditUserData>().ReverseMap();
        }
    }
}
