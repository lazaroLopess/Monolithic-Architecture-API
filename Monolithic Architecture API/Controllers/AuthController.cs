using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Monolithic_Architecture_API.DTOs.Requests;
using Monolithic_Architecture_API.DTOs.Responses;
using Monolithic_Architecture_API.Identity;
using Monolithic_Architecture_API.Mapping;

namespace Monolithic_Architecture_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        public AuthController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest registerModel)
        {
            var userExists = await _userManager.FindByEmailAsync(registerModel.Email);
            if (userExists is not null)
            {
                return Conflict(new ApiResponse<object>
                {
                    Message = "This email is already registered",
                    StatusCode = StatusCodes.Status409Conflict
                });
            }
            var applicationUser = registerModel.ToApplicationUser();
            var result = await _userManager.CreateAsync(applicationUser, registerModel.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return StatusCode(StatusCodes.Status201Created, new ApiResponse<RegisterUserRequest>()
            {
                Message = "user created successfully",
                StatusCode = StatusCodes.Status201Created,
                Data = registerModel
            });
        }
        public async Task<IActionResult> Login([FromBody] LoginUserRequest loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user is null) return NotFound(new ApiResponse<LoginUserRequest>
            {
                Message = $"user with email:{loginModel.Email} not found",
                StatusCode = StatusCodes.Status404NotFound
            });
            if (await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                return Ok("logged");
            }
            return Unauthorized("incorrect password");
        }
    }
}
