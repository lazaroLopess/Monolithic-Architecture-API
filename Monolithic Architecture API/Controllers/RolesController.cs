using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monolithic_Architecture_API.DTOs.Requests;
using Monolithic_Architecture_API.DTOs.Responses;
using Monolithic_Architecture_API.Identity;
using System.Data;

namespace Monolithic_Architecture_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<ApplicationUser> _userManager;
        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        [HttpGet]
        [Authorize(Policy = "SuperAdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get()
        {
            var roles = await _roleManager.Roles
                .Select(r => new RoleResponse { Id = r.Id, Name = r.Name! })
                .ToListAsync();
            return Ok(new ApiResponseGeneric<List<RoleResponse>>
            {
                Message = "roles retrieved successfully",
                StatusCode = StatusCodes.Status200OK,
                Data = roles
            });
        }
        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Post([FromBody] CreateRoleRequest role)
        {
            if (string.IsNullOrWhiteSpace(role.RoleName))
            {
                return BadRequest(new ApiResponse
                {
                    Message = "Role name is required",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            var roleExists = await _roleManager.RoleExistsAsync(role.RoleName);
            if (roleExists)
            {
                return Conflict(new ApiResponse
                {
                    Message = "role already exists",
                    StatusCode = StatusCodes.Status409Conflict
                });
            }
            var result = await _roleManager.CreateAsync(new IdentityRole(role.RoleName));
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            return StatusCode(StatusCodes.Status201Created, new ApiResponse
            {
                Message = "role created successfully",
                StatusCode = StatusCodes.Status200OK
            });
        }
        [HttpPut]
        [Route("{id}")]
        [Authorize(Policy = "SuperAdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Put(string id, [FromBody] UpdateRoleRequest updateRoleRequest)
        {
            if (string.IsNullOrWhiteSpace(updateRoleRequest.NewName))
            {
                return BadRequest(new ApiResponse
                {
                    Message = "role name is required",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound(new ApiResponse
                {
                    Message = "role not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            var existingRole = await _roleManager.FindByNameAsync(updateRoleRequest.NewName);

            if (existingRole is not null && existingRole.Id != role.Id)
            {
                return Conflict(new ApiResponse
                {
                    Message = "role already exists",
                    StatusCode = StatusCodes.Status409Conflict
                });
            }

            role.Name = updateRoleRequest.NewName;

            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            return Ok(new ApiResponse
            {
                Message = "role updated successfully",
                StatusCode = StatusCodes.Status200OK
            });
        }
        [HttpDelete]
        [Route("{id}")]
        [Authorize(Policy = "SuperAdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if(role == null)
            {
                return NotFound(new ApiResponse
                {
                    Message = "role not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }
            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            return Ok(new ApiResponse
            {
                Message = "role deleted successfully",
                StatusCode = StatusCodes.Status200OK
            });
        }
        [HttpPost]
        [Route("assignrole")]
        [Authorize(Policy = "SuperAdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleRequest addToRole)
        {
            if (string.IsNullOrWhiteSpace(addToRole.Id) ||
                string.IsNullOrWhiteSpace(addToRole.Role))
            {
                return BadRequest(new ApiResponse
                {
                    Message = "user id and role are required",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var role = await _roleManager.FindByNameAsync(addToRole.Role);

            if (role == null)
            {
                return NotFound(new ApiResponse
                {
                    Message = "role not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            var user = await _userManager.FindByIdAsync(addToRole.Id);

            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Message = "user not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            if (await _userManager.IsInRoleAsync(user, role.Name!))
            {
                return Conflict(new ApiResponse
                {
                    Message = "user already has this role",
                    StatusCode = StatusCodes.Status409Conflict
                });
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name!);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            return Ok(new ApiResponse
            {
                Message = "role assigned successfully",
                StatusCode = StatusCodes.Status200OK
            });
        }
        [HttpPost]
        [Route("removerole")]
        [Authorize(Policy = "SuperAdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RemoveRole([FromBody] UserRoleRequest removeToRole)
        {
            if (string.IsNullOrWhiteSpace(removeToRole.Id) ||
                string.IsNullOrWhiteSpace(removeToRole.Role))
            {
                return BadRequest(new ApiResponse
                {
                    Message = "user id and role are required",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var role = await _roleManager.FindByNameAsync(removeToRole.Role);

            if (role == null)
            {
                return NotFound(new ApiResponse
                {
                    Message = "role not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            var user = await _userManager.FindByIdAsync(removeToRole.Id);

            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Message = "user not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            if (!await _userManager.IsInRoleAsync(user, role.Name!))
            {
                return Conflict(new ApiResponse
                {
                    Message = "User does not have this role",
                    StatusCode = StatusCodes.Status409Conflict
                });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            return Ok(new ApiResponse
            {
                Message = "role removed successfully",
                StatusCode = StatusCodes.Status200OK
            });
        }
    }
}
