using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using webChat.Data;
using webChat.Models;

namespace webChat.Pages.Posts;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DetailsModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public Post Post { get; set; } = default!;

    public List<Comment> Comments { get; set; } = new();

    [BindProperty]
    public string NewComment { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // 1. Juntar a tabela do Utilizador (.Include)
        var post = await _context.Posts
            .Include(p => p.User) 
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        // 2. Passar a foto atual e o nome do Utilizador para o Post
        if (post.User != null)
        {
            post.ProfileImage = post.User.ProfileImageUrl;
            post.AuthorName = post.User.UserName; 
        }

        Post = post;

        Comments = await _context.Comments
            .Where(c => c.PostId == id)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var comment = new Comment
        {
            Content = NewComment,
            CreatedAt = DateTime.UtcNow,
            PostId = id,
            UserId = user.Id,
            AuthorName = user.UserName ?? "Unknown",
            ProfileImage = user.ProfileImageUrl
                ?? "/images/avatars/default-avatar.png"
        };

        _context.Comments.Add(comment);

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId, int id)
    {
        var comment = await _context.Comments.FindAsync(commentId);

        if (comment == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        if (comment.UserId != userId)
        {
            return Forbid();
        }

        _context.Comments.Remove(comment);

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }
}