using Monolithic_Architecture_API.DTOs.Requests;
using Monolithic_Architecture_API.DTOs.Responses;
using Monolithic_Architecture_API.Identity;

namespace Monolithic_Architecture_API.Mapping
{
    public static class UserMappings
    {
        public static ApplicationUser ToApplicationUser(this RegisterUserRequest request)
        {
            return new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
            };
        }
        public static UserResponse ToUserResponse(ApplicationUser user)
        {
            return new UserResponse
            {
                Id = user.Id!,
                Email = user.Email!,
                UserName = user.UserName!
            };
        }
    }
}
