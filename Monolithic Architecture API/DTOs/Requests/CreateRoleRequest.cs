using System.ComponentModel.DataAnnotations;

namespace Monolithic_Architecture_API.DTOs.Requests
{
    public class CreateRoleRequest
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}
