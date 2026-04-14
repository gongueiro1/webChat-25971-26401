using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using webChat.Data;
using webChat.Models;

namespace webChat.Pages.Posts;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public List<Post> Posts { get; set; } = new();

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {
        Posts = await _context.Posts
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}