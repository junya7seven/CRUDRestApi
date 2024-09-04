using System.ComponentModel.DataAnnotations;

namespace CRUDRestApi.Models
{
    public class UserModel
    {
        [Required]
        [MaxLength(128, ErrorMessage = "First Name cannot exceed 128 characters.")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(128, ErrorMessage = "Last Name cannot exceed 128 characters.")]
        public string LastName { get; set; }

        [Required]
        [MaxLength(128, ErrorMessage = "User Name cannot exceed 128 characters.")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [MaxLength(32, ErrorMessage = "Password cannot exceed 32 characters.")]
        public string Password { get; set; }
    }

}
