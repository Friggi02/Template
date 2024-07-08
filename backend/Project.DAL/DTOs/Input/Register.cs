using System.ComponentModel.DataAnnotations;

namespace Project.DAL.DTOs.Input
{
    public class Register
    {
        [Required, MinLength(1), MaxLength(50)]
        public required string Username { get; set; }
        [EmailAddress, Required, MaxLength(50)]
        public required string Email { get; set; }
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$"), MaxLength(50)]
        public required string Password { get; set; }
    }
}