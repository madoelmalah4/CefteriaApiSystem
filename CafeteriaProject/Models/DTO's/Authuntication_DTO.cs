using System.ComponentModel.DataAnnotations;

namespace CafeteriaProject.Models.DTO_s
{
    public class RegisterRequest
    {
        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } 

        [Required]
        public string Password { get; set; }
    }


}
