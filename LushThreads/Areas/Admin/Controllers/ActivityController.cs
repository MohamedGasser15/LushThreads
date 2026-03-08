using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LushThreads.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing admin activity logs.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = SD.Admin)]
    public class ActivityController : Controller
    {
        #region Fields

        private readonly IAdminActivityService _adminActivityService;
        private readonly UserManager<ApplicationUser> _userManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityController"/> class.
        /// </summary>
        /// <param name="adminActivityService">Service for admin activity operations.</param>
        /// <param name="userManager">Identity user manager.</param>
        public ActivityController(
            IAdminActivityService adminActivityService,
            UserManager<ApplicationUser> userManager)
        {
            _adminActivityService = adminActivityService;
            _userManager = userManager;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Displays the list of all admin activities.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var activities = await _adminActivityService.GetAllActivitiesAsync();
            return View(activities);
        }

        #endregion
    }
}