using System;

namespace Domain
{
    public class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public AppUser Author { get; set; }
        public Activity Activity { get; set; }
        //Date times of comments will be stored in database on normalized UTC time
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}