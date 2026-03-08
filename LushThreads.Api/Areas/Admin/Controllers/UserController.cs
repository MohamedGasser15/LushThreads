using AutoMapper;
using LushThreads.Application.DTOs.User;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LushThreads.Api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = SD.Admin)]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all users with their roles.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(userDtos);
        }

        /// <summary>
        /// Retrieves a specific user by ID for editing (detailed).
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailDto>> GetUser(string id)
        {
            var user = await _userService.GetUserForEditAsync(id);
            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found." });

            var userDetailDto = _mapper.Map<UserDetailDto>(user);
            // RoleList is already populated by the service
            return Ok(userDetailDto);
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateDto)
        {
            if (id != updateDto.Id)
                return BadRequest(new { message = "ID in URL does not match ID in body." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized(new { message = "User identifier not found in token." });

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                // Map DTO to ApplicationUser
                var user = _mapper.Map<ApplicationUser>(updateDto);

                var success = await _userService.UpdateUserAsync(user, currentUserId, ipAddress);

                if (!success)
                    return BadRequest(new { message = "Failed to update user." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the user.", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a user by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized(new { message = "User identifier not found in token." });

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var success = await _userService.DeleteUserAsync(id, currentUserId, ipAddress);

                if (!success)
                    return NotFound(new { message = $"User with ID {id} not found or could not be deleted." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user.", error = ex.Message });
            }
        }

        /// <summary>
        /// Locks or unlocks a user account.
        /// </summary>
        [HttpPost("{id}/lock-unlock")]
        public async Task<ActionResult<LockUnlockResultDto>> LockUnlockUser(string id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized(new { message = "User identifier not found in token." });

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var result = await _userService.LockUnlockUserAsync(id, currentUserId, ipAddress);

                var resultDto = new LockUnlockResultDto
                {
                    Success = result.Success,
                    IsLocked = result.IsLocked,
                    UserName = result.UserName,
                    Message = result.Success
                        ? (result.IsLocked ? $"User '{result.UserName}' locked successfully." : $"User '{result.UserName}' unlocked successfully.")
                        : "Operation failed."
                };

                if (!result.Success)
                    return BadRequest(resultDto);

                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while toggling user lock status.", error = ex.Message });
            }
        }
    }
}