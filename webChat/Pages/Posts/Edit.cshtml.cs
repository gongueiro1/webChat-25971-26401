using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using webChat.Data;
using webChat.Models;

namespace webChat.Pages.Posts;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public Post Post { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var post = await _context.Posts.FirstOrDefaultAsync(m => m.Id == id);
        if (post == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        
        // Regra de segurança: Só o dono do post o pode editar!
        if (post.UserId != userId) return Forbid(); 

        Post = post;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var postToUpdate = await _context.Posts.FirstOrDefaultAsync(p => p.Id == Post.Id);
        if (postToUpdate == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (postToUpdate.UserId != userId) return Forbid();

        // Atualizar apenas o título e o conteúdo
        postToUpdate.Title = Post.Title;
        postToUpdate.Content = Post.Content;

        await _context.SaveChangesAsync();

        // Volta para a página de detalhes depois de gravar
        return RedirectToPage("./Details", new { id = Post.Id });
    }
}