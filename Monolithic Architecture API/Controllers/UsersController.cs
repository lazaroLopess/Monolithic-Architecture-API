using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monolithic_Architecture_API.DTOs.Requests;
using Monolithic_Architecture_API.DTOs.Responses;
using Monolithic_Architecture_API.Identity;
using Monolithic_Architecture_API.Mapping;
using System.Security.Claims;

namespace Monolithic_Architecture_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet]
        [Authorize(Policy = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get()
        {
            var users = await _userManager.Users
                .Select(u => UserMappings.ToUserResponse(u))
                .ToListAsync();
            return Ok(new ApiResponseGeneric<List<UserResponse>>
            {
                Message = "Users retrieved successfully",
                StatusCode = StatusCodes.Status200OK,
                Data = users
            });
        }
        [HttpGet]
        [Route("{identifier}")]
        [Authorize(Policy = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(string identifier)
        {
            var user = await FindUserByIdentifierAsync(identifier);
            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Message = "user not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }
            var responseUser = UserMappings.ToUserResponse(user);
            return Ok(new ApiResponseGeneric<UserResponse>
            {
                Message = "user found successfully",
                StatusCode = StatusCodes.Status200OK,
                Data = responseUser
            });
        }
        [HttpPut]
        [Authorize(Policy = "User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put([FromBody] UpdateUserRequest updateUserRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return NotFound(new ApiResponse
                {
                    Message = "user not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }
            var newEmail = updateUserRequest.Email;
            var existingUser = await _userManager.FindByEmailAsync(newEmail);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return Conflict(new ApiResponse
                {
                    Message = "email already exists",
                    StatusCode = StatusCodes.Status409Conflict
                });
            }
            var emailResult = await _userManager.SetEmailAsync(user, newEmail);
            if (!emailResult.Succeeded)
            {
                return BadRequest(new ApiResponse
                {
                    Message = string.Join(", ", emailResult.Errors.Select(e => e.Description)),
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            var newUsername = updateUserRequest.UserName;
            var usernameResult = await _userManager.SetUserNameAsync(user, newUsername);
            if (!usernameResult.Succeeded)
            {
                return BadRequest(new ApiResponse
                {
                    Message = string.Join(", ", usernameResult.Errors.Select(e => e.Description)),
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            return Ok(new ApiResponse
            {
                Message = "user updated successfully",
                StatusCode = StatusCodes.Status200OK
            });
        }
        [HttpPut("{id}")]
        [Authorize(Policy = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put([FromRoute] string id, [FromBody] UpdateUserRequest updateUserRequest)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
            {
                return NotFound(new ApiResponse
                {
                    Message = "User not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(updateUserRequest.Email);

            if (existingUser is not null && existingUser.Id != user.Id)
            {
                return Conflict(new ApiResponse
                {
                    Message = "Email already exists",
                    StatusCode = StatusCodes.Status409Conflict
                });
            }

            var emailResult = await _userManager.SetEmailAsync(user, updateUserRequest.Email);

            if (!emailResult.Succeeded)
            {
                return BadRequest(new ApiResponse
                {
                    Message = string.Join(", ", emailResult.Errors.Select(e => e.Description)),
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var usernameResult = await _userManager.SetUserNameAsync(user, updateUserRequest.UserName);

            if (!usernameResult.Succeeded)
            {
                return BadRequest(new ApiResponse
                {
                    Message = string.Join(", ", usernameResult.Errors.Select(e => e.Description)),
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            return Ok(new ApiResponse
            {
                Message = "User updated successfully",
                StatusCode = StatusCodes.Status200OK
            });
        }
        [HttpDelete("{identifier}")]
        [Authorize(Policy = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string identifier)
        {
            var user = await FindUserByIdentifierAsync(identifier);
            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Message = "user not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }
            var result = await _userManager.DeleteAsync(user);
            if(!result.Succeeded)
            {
                return BadRequest(new ApiResponse
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            return Ok(new ApiResponse
            {
                Message = "user deleted successfully",
                StatusCode = StatusCodes.Status200OK
            });
        }
        private async Task<ApplicationUser?> FindUserByIdentifierAsync(string identifier)
        {
            if (Guid.TryParse(identifier, out _))
            {
                return await _userManager.FindByIdAsync(identifier);
            }
            return await _userManager.FindByEmailAsync(identifier);
        }
    }
}

