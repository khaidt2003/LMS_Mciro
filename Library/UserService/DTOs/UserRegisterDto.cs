using System.ComponentModel.DataAnnotations;
using UserService.Models;

namespace UserService.DTOs
{
    public class UserRegisterDto
    {
        [Required]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters.")]
        public required string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string Password { get; set; }
        
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string Email { get; set; }
        
        public int Roles { get; set; } = Role.User;
    }
}
