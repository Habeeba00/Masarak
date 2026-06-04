using System.Security.Claims;

namespace Masarak.Services
{
    public interface IJwtService
    {
        (string token, DateTime expiry, string jwtId) GenerateAccessToken(int userId, string email, string fullName, string role);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        string GenerateRefreshToken();
    }
}
