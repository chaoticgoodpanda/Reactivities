namespace Domain
{
    public class UserFollowing
    {
        //Observer = follower ; Target = who's being followed
        //Since Follower and Following are spelled very similarly, so can be confusing
        public string ObserverId { get; set; }
        public AppUser Observer { get; set; }
        public string TargetId { get; set; }
        public AppUser Target { get; set; }
    }
}