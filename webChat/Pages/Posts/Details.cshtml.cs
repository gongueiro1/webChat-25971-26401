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
    
    [BindProperty]
    public string ReplyContent { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        if (post.User != null)
        {
            post.ProfileImage = post.User.ProfileImageUrl;
            post.AuthorName = post.User.UserName;
        }

        Post = post;

        Comments = await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Replies)
            .ThenInclude(r => r.User)
            .Where(c => c.PostId == id && c.ParentCommentId == null)
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
            UserId = user.Id
        };

        _context.Comments.Add(comment);

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId, int id)
    {
        var comment = await _context.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        if (comment.UserId != userId)
        {
            return Forbid();
        }

        if (comment.Replies.Any())
        {
            _context.Comments.RemoveRange(comment.Replies);
        }

        _context.Comments.Remove(comment);

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    // NOVO MÉTODO PARA APAGAR APENAS UMA RESPOSTA
    public async Task<IActionResult> OnPostDeleteReplyAsync(int replyId, int id)
    {
        var reply = await _context.Comments.FindAsync(replyId);

        if (reply == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        if (reply.UserId != userId)
        {
            return Forbid();
        }

        _context.Comments.Remove(reply);

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }
    
    public async Task<IActionResult> OnPostReplyAsync(
        int id,
        int parentCommentId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var reply = new Comment
        {
            Content = ReplyContent,
            CreatedAt = DateTime.UtcNow,
            PostId = id,
            ParentCommentId = parentCommentId,
            UserId = user.Id
        };

        _context.Comments.Add(reply);

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }
    
    public async Task<IActionResult> OnPostDeletePostAsync(int id)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        if (post.UserId != userId)
        {
            return Forbid();
        }

        var comments = await _context.Comments
            .Where(c => c.PostId == id)
            .ToListAsync();

        _context.Comments.RemoveRange(comments);

        var supports = await _context.PostSupports
            .Where(s => s.PostId == id)
            .ToListAsync();

        _context.PostSupports.RemoveRange(supports);

        _context.Posts.Remove(post);

        await _context.SaveChangesAsync();

        return RedirectToPage("/Posts/Index");
    }
    
    public async Task<IActionResult> OnPostCommentAjaxAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return new JsonResult(new { success = false });
        }

        var comment = new Comment
        {
            Content = NewComment,
            CreatedAt = DateTime.UtcNow,
            PostId = id,
            UserId = user.Id
        };

        _context.Comments.Add(comment);

        await _context.SaveChangesAsync();

        return new JsonResult(new
        {
            success = true,
            commentId = comment.Id,
            username = user.UserName,
            avatar = user.ProfileImageUrl ?? "/images/avatars/default-avatar.png",
            content = comment.Content,
            date = comment.CreatedAt.ToString("g")
        });
    }
    
    public async Task<IActionResult> OnPostReplyAjaxAsync(
        int id,
        int parentCommentId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return new JsonResult(new
            {
                success = false
            });
        }

        var reply = new Comment
        {
            Content = ReplyContent,
            CreatedAt = DateTime.UtcNow,
            PostId = id,
            ParentCommentId = parentCommentId,
            UserId = user.Id
        };

        _context.Comments.Add(reply);

        await _context.SaveChangesAsync();

        return new JsonResult(new
        {
            success = true,
            commentId = reply.Id,
            username = user.UserName,
            avatar = user.ProfileImageUrl ?? "/images/avatars/default-avatar.png",
            content = reply.Content,
            date = reply.CreatedAt.ToString("g")
        });
    }
}