using System.ComponentModel.DataAnnotations;

namespace Monolithic_Architecture_API.DTOs.Requests
{
    public class UserRoleRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
