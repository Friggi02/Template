using Project.DAL.Utils;

namespace Project.DAL.Entities
{
    public sealed class Role(Guid id, string name) : Enumeration<Role>(id, name)
    {
        public static readonly Role Registered = new(Guid.NewGuid(), "Registered");
        public static readonly Role Admin = new(Guid.NewGuid(), "Admin");

        public ICollection<Permission> Permissions { get; set; } = [];
        public ICollection<User> Users { get; set; } = [];
    }
}