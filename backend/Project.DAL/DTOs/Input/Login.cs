using System.ComponentModel.DataAnnotations;

namespace Project.DAL.DTOs.Input
{
    public class Login
    {
        [EmailAddress, Required, MaxLength(50)]
        public required string Email { get; set; }
        [Required, RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$"), MaxLength(50)]
        public required string Password { get; set; }
    }
}