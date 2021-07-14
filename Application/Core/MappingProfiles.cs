using System.Linq;
using Application.Activities;
using Application.Comments;
using Application.Profiles;
using Domain;
using Microsoft.AspNetCore.Mvc.Rendering;
using Profile = AutoMapper.Profile;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            //to be able to pull the token of the current user since cannot inject into constructor MappingProfiles()
            string currentUsername = null;
            
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
            CreateMap<ActivityAttendee, AttendeeDTO>()
                .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.AppUser.DisplayName))
                .ForMember(d => d.Username, o => o.MapFrom(s => s.AppUser.UserName))
                .ForMember(d => d.Bio, o => o.MapFrom(s => s.AppUser.Bio))
                //setting image pulls as well
                .ForMember(d => d.Image, o => o.MapFrom(s => s.AppUser.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(d => d.FollowersCount, o => o.MapFrom(s => s.AppUser.Followers.Count))
                .ForMember(d => d.FollowingCount, o => o.MapFrom(s => s.AppUser.Followings.Count))
                //checking if the currently logged in user is a follower of this particular other user
                .ForMember(d => d.Following, o =>
                    o.MapFrom(s => s.AppUser.Followers.Any(x => x.Observer.UserName == currentUsername)));
                
            //in order to reset main photos for user profiles
            //maps observers and targets for followings
            CreateMap<AppUser, Profiles.Profile>()
                .ForMember(d => d.Image, o => o.MapFrom(s => s.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(d => d.FollowersCount, o => o.MapFrom(s => s.Followers.Count))
                .ForMember(d => d.FollowingCount, o => o.MapFrom(s => s.Followings.Count))
                //checking if the currently logged in user is a follower of this particular other user
                .ForMember(d => d.Following, o =>
                    o.MapFrom(s => s.Followers.Any(x => x.Observer.UserName == currentUsername)));
            
            
            CreateMap<Comment, CommentsDTO>()
                .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.Author.DisplayName))
                .ForMember(d => d.Username, o => o.MapFrom(s => s.Author.UserName))
                .ForMember(d => d.Image, o => o.MapFrom(s => s.Author.Photos.FirstOrDefault(x => x.IsMain).Url));

            CreateMap<ActivityAttendee, UserActivityDTO>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Activity.Id))
                .ForMember(d => d.Date, o => o.MapFrom(s => s.Activity.Date))
                .ForMember(d => d.Title, o => o.MapFrom(s => s.Activity.Title))
                .ForMember(d => d.Category, o => o.MapFrom(s => s.Activity.Category))
                .ForMember(d => d.HostUserName, o => o.MapFrom(s
                    => s.Activity.Attendees.FirstOrDefault(x => x.IsHost).AppUser.UserName));
        }
    }
}