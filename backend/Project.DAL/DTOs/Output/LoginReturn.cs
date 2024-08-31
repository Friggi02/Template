using Project.DAL.DTOs.Input;
using Project.DAL.Entities;

namespace Project.DAL.DTOs.Output
{
    public class LoginReturn
    {
        public required Tokens Tokens { get; set; }
        public required MappedUser User { get; set; }
    }
}