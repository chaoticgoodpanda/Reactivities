using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Domain
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public ICollection<ActivityAttendee> Activities { get; set; }
        public ICollection<Photo> Photos { get; set; }
        //who is the current user following
        public ICollection<UserFollowing> Followings { get; set; }
        //who is following the current user
        public ICollection<UserFollowing> Followers { get; set; }
        //we'll have a list we can add a token to for refreshing tokens
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}