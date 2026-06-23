using System.ComponentModel.DataAnnotations;

namespace Monolithic_Architecture_API.DTOs.Requests
{
    public class AddToRoleRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
