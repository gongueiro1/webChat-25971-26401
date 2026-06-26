#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using webChat.Data;
using webChat.Models;

namespace webChat.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly ApplicationDbContext _context;

        public DeletePersonalDataModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);

            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            var userId = await _userManager.GetUserIdAsync(user);

            var userPosts = await _context.Posts
                .Where(p => p.UserId == userId)
                .ToListAsync();

            foreach (var post in userPosts)
            {
                var postComments = await _context.Comments
                    .Where(c => c.PostId == post.Id)
                    .ToListAsync();

                _context.Comments.RemoveRange(postComments);

                var postSupports = await _context.PostSupports
                    .Where(s => s.PostId == post.Id)
                    .ToListAsync();

                _context.PostSupports.RemoveRange(postSupports);

                var postReports = await _context.Reports
                    .Where(r => r.PostId == post.Id)
                    .ToListAsync();

                _context.Reports.RemoveRange(postReports);

                _context.Posts.Remove(post);
            }

            var userComments = await _context.Comments
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.Comments.RemoveRange(userComments);

            var userSupports = await _context.PostSupports
                .Where(s => s.UserId == userId)
                .ToListAsync();

            _context.PostSupports.RemoveRange(userSupports);

            var userReports = await _context.Reports
                .Where(r => r.ReporterUserId == userId)
                .ToListAsync();

            _context.Reports.RemoveRange(userReports);

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Unexpected error occurred deleting user.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }
    }
}