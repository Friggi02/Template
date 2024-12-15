using System.ComponentModel.DataAnnotations;

namespace Project.DAL.DTOs.Input
{
    public class SelfDelete
    {
        [Required, RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$"), MaxLength(50)]
        public required string CurrentPassword { get; set; }
    }
}