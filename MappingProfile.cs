using CompanyEmployees.IDP.Entities.ViewModels;
using CompanyEmployees.IDP.Entities;
using AutoMapper;

namespace CompanyEmployees.IDP
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserRegistrationModel, User>()
            .ForMember(u => u.UserName, opt => opt.MapFrom(x => x.Email));
        }
    }
}
