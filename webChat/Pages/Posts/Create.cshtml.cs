using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webChat.Data;
using webChat.Models;

namespace webChat.Pages.Posts;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    [BindProperty]
    public Post Post { get; set; } = new();

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        _context.Posts.Add(Post);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}