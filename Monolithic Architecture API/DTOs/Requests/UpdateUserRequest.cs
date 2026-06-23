using System.ComponentModel.DataAnnotations;

namespace Monolithic_Architecture_API.DTOs.Requests
{
    public class UpdateUserRequest
    {
        [Required]
        public string UserName {  get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
