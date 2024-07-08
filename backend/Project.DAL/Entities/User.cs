using Project.DAL.Utils;
using System.ComponentModel.DataAnnotations;

namespace Project.DAL.Entities
{
    public class User : BaseEntity
    {
        [MaxLength(50)]
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public int AccessFailedCount { get; set; }
        [MaxLength(50)]
        public required string Email { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string? RefreshToken { get; set; }
        [MaxLength(50)]
        public string? Name { get; set; }
        [MaxLength(50)]
        public string? Surname { get; set; }
        [Url]
        public string? ProfilePic { get; set; }
        public ICollection<Role> Roles { get; set; } = [];
        public bool IsLockedout() => LockoutEnd != null && LockoutEnd > DateTime.UtcNow;
    }
}