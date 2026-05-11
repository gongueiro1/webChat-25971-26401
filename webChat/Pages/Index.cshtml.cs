using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using webChat.Data;
using webChat.Models;

namespace webChat.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    // A lista que vai guardar os posts para o HTML ler
    public IList<Post> Posts { get; set; } = default!;

    public async Task OnGetAsync()
    {
        if (_context.Posts != null)
        {
            // Vai buscar à base de dados os posts, ordenados pelos mais recentes
            Posts = await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}