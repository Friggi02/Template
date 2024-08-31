using Project.DAL.Entities;

namespace Project.DAL.DTOs.Output
{
    public class MappedUser
    {
        public required Guid Id { get; set; }
        public required string Email { get; set; }
        public string? Username { get; set; }
        public bool Active { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? ProfilePic { get; set; }
        public string[] Roles { get; set; } = [];
    }
}