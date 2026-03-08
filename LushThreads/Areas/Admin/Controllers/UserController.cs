using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LushThreads.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Admin)]
    public class UserController : Controller
    {
        #region Fields

        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        #endregion

        #region Constructor

        public UserController(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Displays the list of all users with their roles.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        /// <summary>
        /// Displays the form for editing a user's details.
        /// </summary>
        public async Task<IActionResult> Edit(string userId)
        {
            var user = await _userService.GetUserForEditAsync(userId);
            if (user == null)
                return NotFound();

            return View(user);
        }

        /// <summary>
        /// Updates a user's details and role.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser user)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return NotFound();

            bool success = await _userService.UpdateUserAsync(
                user,
                currentUser.Id,
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            if (success)
            {
                TempData["Success"] = $"User ('{user.Name}') updated successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = "Oops! Something went wrong. Please try again.";
                user.RoleList = (await _userService.GetUserForEditAsync(user.Id))?.RoleList;
                return View(user);
            }
        }

        /// <summary>
        /// Deletes a user and their associated devices.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return NotFound();

            bool success = await _userService.DeleteUserAsync(
                userId,
                currentUser.Id,
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            if (success)
            {
                TempData["Success"] = "User deleted successfully!";
            }
            else
            {
                TempData["Error"] = "An error occurred while deleting the user. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Locks or unlocks a user account.
        /// </summary>
        public async Task<IActionResult> LockUnlock(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return NotFound();

            var result = await _userService.LockUnlockUserAsync(
                userId,
                currentUser.Id,
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            if (result.Success)
            {
                if (result.IsLocked)
                {
                    TempData["Success"] = $"Successfully locked user ({result.UserName}) for 10 days!";
                }
                else
                {
                    TempData["Success"] = $"Successfully unlocked user ({result.UserName})!";
                }
            }
            else
            {
                TempData["Error"] = "User not found! The account may have been deleted or doesn't exist.";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}