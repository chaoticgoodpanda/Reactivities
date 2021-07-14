using System;
using System.Text.Json.Serialization;

namespace Application.Profiles
{
    public class UserActivityDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
        
        //property that helps us but that we don't want to return to client, use [JsonIgnore]
        [JsonIgnore]
        public string HostUserName { get; set; }
    }
}