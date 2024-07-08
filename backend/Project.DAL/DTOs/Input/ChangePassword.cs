using System.ComponentModel.DataAnnotations;

namespace Project.DAL.DTOs.Input
{
    public class ChangePassword
    {
        [Required, RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$"), MaxLength(50)]
        public required string CurrentPassword { get; set; }
        [Required, RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$"), MaxLength(50)]
        public required string NewPassword { get; set; }
    }
}