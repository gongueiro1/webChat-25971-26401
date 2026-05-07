using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webChat.Data;
using webChat.Models;

namespace webChat.Pages.Posts;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    [BindProperty]
    public Post Post { get; set; } = new();

    public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            return Forbid();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            return Forbid();

        if (!ModelState.IsValid)
            return Page();

        Post.UserId = user.Id;
        Post.AuthorName = user.UserName ?? "Unknown";
        Post.CreatedAt = DateTime.UtcNow;

        _context.Posts.Add(Post);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Posts/Index");
    }
}