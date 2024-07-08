using System.ComponentModel.DataAnnotations;

namespace Project.DAL.DTOs.Input
{
    public class ChangeUserName
    {
        [Required, RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$"), MaxLength(50)]
        public required string CurrentPassword { get; set; }
        [Required, MinLength(1), MaxLength(50)]
        public required string NewUserName { get; set; }
    }
}