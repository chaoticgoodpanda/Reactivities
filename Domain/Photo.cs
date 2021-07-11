namespace Domain
{
    public class Photo
    {
        //same as the id we get back from Cloudinary
        public string Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        
    }
}