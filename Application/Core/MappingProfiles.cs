using System.Linq;
using Application.Activities;
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
            //mapping from an Activity to ActivityDTO
            CreateMap<Activity, ActivityDTO>()
                //hostusername and profiles are null unless we explicitly specify them
                .ForMember(d => d.HostUsername, 
                    o => o.MapFrom(
                        s => s.Attendees.FirstOrDefault(x => x.IsHost).AppUser.UserName));
            //ActivityDTO has Attendees property - need to map from ActivityAttendee to our Profiles object
            //image is ignored for time being
            CreateMap<ActivityAttendee, Profiles.Profile>()
                .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.AppUser.DisplayName))
                .ForMember(d => d.Username, o => o.MapFrom(s => s.AppUser.UserName))
                .ForMember(d => d.Bio, o => o.MapFrom(s => s.AppUser.Bio));
        }
    }
}