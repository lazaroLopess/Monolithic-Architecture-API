using System.ComponentModel.DataAnnotations;

namespace Monolithic_Architecture_API.DTOs.Requests
{
    public class RefreshTokenUserRequest
    {
        [Required]
        public string AccessTokenExpired { get; set; } = string.Empty;
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
