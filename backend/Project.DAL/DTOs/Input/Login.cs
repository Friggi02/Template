using System.ComponentModel.DataAnnotations;

namespace Project.DAL.DTOs.Input
{
    public class Login
    {
        [Required, MaxLength(50)]
        public required string EmailOrUsername { get; set; }
        [Required, RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$"), MaxLength(50)]
        public required string Password { get; set; }
    }
}