using Project.DAL.Entities;

namespace Project.DAL.Jwt
{
    public interface IJwtProvider
    {
        public Task<string> GenerateAccessToken(User user);
        public string GenerateRefreshToken(User user);
    }
}