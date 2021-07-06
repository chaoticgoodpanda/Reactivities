using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string DisplayName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [RegularExpression("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{4,30}$", 
            ErrorMessage = "Password must have at least 1 lowercase, 1 uppercase, 4-30 characters")]  
        public string Password { get; set; }
        
        [Required]
        public string Username { get; set; }
    }
}