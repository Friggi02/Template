using Project.DAL.Entities;

namespace Project.DAL.DTOs.Output
{
    public class LoginReturn
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required MappedUser User { get; set; }
    }
}