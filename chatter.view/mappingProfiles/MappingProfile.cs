using AutoMapper;
using chatter.core.models;
using chatter.view.models;

namespace chatter.view.mappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegistrationViewModel, User>();
            CreateMap<LoginViewModel, User>();
        }
    }
}
