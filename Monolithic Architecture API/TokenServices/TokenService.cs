using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Monolithic_Architecture_API.TokenServices
{
    public class TokenService : ITokenService
    {
        public (JwtSecurityToken Token, DateTime ExpiresAt) GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration config)
        {
            var key = config.GetSection("JWT").GetValue<string>("SecretKey") ?? throw new InvalidOperationException("JWT secret key is not configured");
            var privateKey = Encoding.UTF8.GetBytes(key);
            var signingCredentials = new SigningCredentials(key: new SymmetricSecurityKey(privateKey), algorithm: SecurityAlgorithms.HmacSha256Signature);
            var expiresAt = DateTime.UtcNow.AddHours(config.GetSection("JWT").GetValue<int>("AccessTokenExpirationHours"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = signingCredentials,
                Expires = expiresAt,
                Issuer = config["JWT:Issuer"],
                Audience = config["JWT:Audience"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            return (token, expiresAt);
        }

        public string GenerateRefreshToken()
        {
            string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return refreshToken;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration config)
        {
            var key = config.GetSection("JWT").GetValue<string>("SecretKey") ?? throw new InvalidOperationException("JWT secret key is not configured");
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = config["JWT:Audience"],
                ValidIssuer = config["JWT:Issuer"],
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuerSigningKey = true
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            if(validatedToken is not JwtSecurityToken )
            {
                throw new SecurityTokenException("invalid token");
            }
            return principal;
        }
    }
}
