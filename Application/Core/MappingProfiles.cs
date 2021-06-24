using AutoMapper;
using Domain;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            //mapping from an activity (Edit() function) to an activity
            CreateMap<Activity, Activity>();
        }
    }
}