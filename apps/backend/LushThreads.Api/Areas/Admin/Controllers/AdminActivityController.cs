using AutoMapper;
using LushThreads.Application.DTOs.AdminActivity;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LushThreads.Api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = SD.Admin)]
    public class AdminActivityController : ControllerBase
    {
        private readonly IAdminActivityService _adminActivityService;
        private readonly IMapper _mapper;

        public AdminActivityController(IAdminActivityService adminActivityService, IMapper mapper)
        {
            _adminActivityService = adminActivityService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all admin activities.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminActivityDto>>> GetActivities()
        {
            var activities = await _adminActivityService.GetAllActivitiesAsync();
            var activityDtos = _mapper.Map<IEnumerable<AdminActivityDto>>(activities);
            return Ok(activityDtos);
        }

        /// <summary>
        /// Retrieves activities for a specific user.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AdminActivityDto>>> GetActivitiesByUser(string userId)
        {
            var activities = await _adminActivityService.GetActivitiesByUserIdAsync(userId);
            var activityDtos = _mapper.Map<IEnumerable<AdminActivityDto>>(activities);
            return Ok(activityDtos);
        }

        /// <summary>
        /// Retrieves activities within a date range.
        /// </summary>
        [HttpGet("daterange")]
        public async Task<ActionResult<IEnumerable<AdminActivityDto>>> GetActivitiesByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var activities = await _adminActivityService.GetActivitiesByDateRangeAsync(startDate, endDate);
            var activityDtos = _mapper.Map<IEnumerable<AdminActivityDto>>(activities);
            return Ok(activityDtos);
        }

        /// <summary>
        /// Deletes activities older than the specified date (admin only).
        /// </summary>
        [HttpDelete("old")]
        public async Task<IActionResult> DeleteOldActivities([FromQuery] DateTime cutoffDate)
        {
            var deletedCount = await _adminActivityService.DeleteOldActivitiesAsync(cutoffDate);
            return Ok(new { message = $"Deleted {deletedCount} old activities." });
        }
    }
}
