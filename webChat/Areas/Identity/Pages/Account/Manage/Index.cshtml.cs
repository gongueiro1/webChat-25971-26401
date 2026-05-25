#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webChat.Models;

namespace webChat.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            public string ProfileImageUrl { get; set; }

            public IFormFile ProfilePicture { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Input = new InputModel
            {
                UserName = userName,
                PhoneNumber = phoneNumber,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.UserName != user.UserName)
            {
                var existingUser = await _userManager.FindByNameAsync(Input.UserName);

                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ModelState.AddModelError("Input.UserName", "This username is already taken.");
                    await LoadAsync(user);
                    return Page();
                }

                var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.UserName);

                if (!setUserNameResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to change username.";
                    return RedirectToPage();
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);

                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.ProfilePicture != null)
            {
                var uniqueFileName = Guid.NewGuid() + "_" + Input.ProfilePicture.FileName;

                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/profiles");

                Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ProfilePicture.CopyToAsync(fileStream);
                }

                user.ProfileImageUrl = "/images/profiles/" + uniqueFileName;

                var updateUserResult = await _userManager.UpdateAsync(user);

                if (!updateUserResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to update profile picture.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Your profile has been updated.";
            return RedirectToPage();
        }
    }
}