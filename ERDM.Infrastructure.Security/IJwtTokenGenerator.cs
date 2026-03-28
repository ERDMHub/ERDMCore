using System.Security.Claims;

namespace ERDM.Infrastructure.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(string userId, string email, IEnumerable<string> roles);
        ClaimsPrincipal ValidateToken(string token);
        string GenerateRefreshToken();
    }
}
