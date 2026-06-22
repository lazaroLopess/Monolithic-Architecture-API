using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Monolithic_Architecture_API.DTOs.Requests;
using Monolithic_Architecture_API.DTOs.Responses;
using Monolithic_Architecture_API.Identity;
using Monolithic_Architecture_API.Mapping;
using Monolithic_Architecture_API.TokenServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Monolithic_Architecture_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private TokenService _tokenService;
        private IConfiguration _configuration;
        public AuthController(UserManager<ApplicationUser> userManager, TokenService tokenService, IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest registerModel)
        {
            var userExists = await _userManager.FindByEmailAsync(registerModel.Email);
            if (userExists is not null)
            {
                return Conflict(new ApiResponse
                {
                    Message = "email already registered",
                    StatusCode = StatusCodes.Status409Conflict
                });
            }
            var applicationUser = registerModel.ToApplicationUser();
            var result = await _userManager.CreateAsync(applicationUser, registerModel.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return StatusCode(StatusCodes.Status201Created, new ApiResponse()
            {
                Message = "user created successfully",
                StatusCode = StatusCodes.Status201Created
            });
        }
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginUserRequest loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, loginModel.Password)) return Unauthorized(new ApiResponse
            {
                Message = "incorrect user or password",
                StatusCode = StatusCodes.Status401Unauthorized
            });
            var roles = await _userManager.GetRolesAsync(user);
            List<Claim> claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id!));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName!));
            claims.Add(new Claim(ClaimTypes.Email, user.Email!));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            var tokenHandler = new JwtSecurityTokenHandler();
            var (token, expiresAt) = _tokenService.GenerateAccessToken(claims, _configuration);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiryTime = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("JWT:RefreshTokenExpirationHours"));
            user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
            user.RefreshToken = refreshToken;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            var loginResponse = new LoginResponse 
            {
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = expiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiryTime
            };
            return Ok(new ApiResponseGeneric<LoginResponse>
            {
                Message = "Login successful",
                StatusCode = StatusCodes.Status200OK,
                Data = loginResponse
            });
        }
        [HttpPost]
        [Route("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenUserRequest refreshTokenUserRequest)
        {
            if(string.IsNullOrWhiteSpace(refreshTokenUserRequest.RefreshToken) || string.IsNullOrEmpty(refreshTokenUserRequest.AccessToken))
            {
                return BadRequest(new ApiResponse
                {
                    Message = "Access token and refresh token are required",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            var accessToken = refreshTokenUserRequest.AccessToken;
            var refreshToken = refreshTokenUserRequest.RefreshToken;
            ClaimsPrincipal principal;
            try
            {
                principal = _tokenService.GetPrincipalFromExpiredToken(accessToken, _configuration);
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new ApiResponse
                {
                    Message = "Invalid access token",
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            }
            string? email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _userManager.FindByEmailAsync(email!);
            if(user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow )
            {
                return Unauthorized(new ApiResponse
                {
                    Message = "invalid access/refresh token",
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var roles = await _userManager.GetRolesAsync(user);
            var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id!));
            claims.Add(new Claim(ClaimTypes.Email, user.Email!));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName!));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var (Token, ExpiresAt) = _tokenService.GenerateAccessToken(claims, _configuration);
            user.RefreshToken = newRefreshToken;
            var refreshTokenExpiryTime = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("JWT:RefreshTokenExpirationHours"));
            user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var dataResponse = new LoginResponse
                {
                    Token = tokenHandler.WriteToken(Token),
                    RefreshToken = newRefreshToken,
                    AccessTokenExpiresAt = ExpiresAt,
                    RefreshTokenExpiresAt = refreshTokenExpiryTime
                };
                return Ok(new ApiResponseGeneric<LoginResponse>
                {
                    Message = "Token refreshed successfully",
                    StatusCode = StatusCodes.Status200OK,
                    Data = dataResponse
                });
            }
            return BadRequest(new ApiResponse
            {
                Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
    }
}
