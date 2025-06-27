using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TravelFinalProject.Models;
using TravelFinalProject.ViewModels.ProfileVM;
using TravelFinalProject.ViewModels.Users;

namespace TravelFinalProject.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileVM
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ChangePassword = new ChangePasswordVM()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(ProfileVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            user.Name = model.Name;
            user.Surname = model.Surname;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            ViewBag.StatusMessage = "Profile updated successfully";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all fields correctly.";
                return RedirectToAction("MyProfile");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.ChangePassword.CurrentPassword, model.ChangePassword.NewPassword);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction("MyProfile");
            }

            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction("MyProfile");
        }



    }
}


