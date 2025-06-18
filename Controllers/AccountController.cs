using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using TravelFinalProject.Models;
using TravelFinalProject.Utilities.Enums;
using TravelFinalProject.ViewModels;
using TravelFinalProject.ViewModels.Users;
using ResetPasswordVM = TravelFinalProject.ViewModels.Users.ResetPasswordVM;

namespace TravelFinalProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }


        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            AppUser user = new AppUser()
            {
                Name = registerVM.Name,
                Surname = registerVM.Surname,
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                City = registerVM.City,
                Country = registerVM.Country,
                DateOfBirth = registerVM.BirthDate,
                PhoneNumber = registerVM.PhoneNumber

            };
            IdentityResult result = await _userManager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);

                };
                return View();
            }
            await _userManager.AddToRoleAsync(user, UserRole.Member.ToString());
            await _signInManager.SignInAsync(user, false);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM, string? returnUrl)
        {
            if (!ModelState.IsValid) return View();
            AppUser user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == loginVM.UserNameOrEmail || u.Email == loginVM.UserNameOrEmail);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Username,Email or Password is incorrect");
                return View();
            }
            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.IsPersistent, true);
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "You are blocked");
                return View();
            }
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Username,Email or Password is incorrect");
                return View();

            }

            if (returnUrl is null) return RedirectToAction(nameof(HomeController.Index), "Home");


            return Redirect(returnUrl);


        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");


        }
        public async Task<IActionResult> CreateRole()
        {
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                if (!await _roleManager.RoleExistsAsync(role.ToString()))
                {
                    await _roleManager.CreateAsync(new IdentityRole
                    {
                        Name = role.ToString()

                    });

                }
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");

        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View(new ForgotPasswordVM());

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                model.EmailSent = true;
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);

            await SendEmailAsync(user.Email, "Reset your password",
                $"<p>Click <a href='{resetLink}'>here</a> to reset your password.</p>");

            model.EmailSent = true;
            return View(model);
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MailMessage();
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;
            message.From = new MailAddress("your-email@gmail.com", "Your App Name");

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password"),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null) return BadRequest();
            return View(new ResetPasswordVM { Token = token, Email = email });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }


    }
}
