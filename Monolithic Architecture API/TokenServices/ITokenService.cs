using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Monolithic_Architecture_API.TokenServices
{
    public interface ITokenService
    {
        (JwtSecurityToken Token, DateTime ExpiresAt) GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration config);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration config);
    }
}
