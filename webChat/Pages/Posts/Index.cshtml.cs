using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using webChat.Data;
using webChat.Models;

namespace webChat.Pages.Posts;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<PostDto> Posts { get; set; } = new();

    public async Task OnGetAsync()
    {
        using var client = new HttpClient();

        var response = await client.GetStringAsync("http://localhost:5030/api/posts");

        Posts = JsonSerializer.Deserialize<List<PostDto>>(
            response,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<PostDto>();

        if (Posts.Any())
        {
            var postIds = Posts.Select(p => p.Id).ToList();

            var allSupports = _context.PostSupports
                .Where(s => postIds.Contains(s.PostId))
                .ToList();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            foreach (var post in Posts)
            {
                post.FormattedSupportCount =
                    allSupports.Count(s => s.PostId == post.Id);

                post.IsSupportedByCurrentUser =
                    userId != null &&
                    allSupports.Any(s =>
                        s.PostId == post.Id &&
                        s.UserId == userId);
            }
        }
    }

    public async Task<IActionResult> OnPostSupportAsync(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var existingSupport = _context.PostSupports
            .FirstOrDefault(s => s.PostId == id && s.UserId == userId);

        if (existingSupport != null)
        {
            _context.PostSupports.Remove(existingSupport);
        }
        else
        {
            _context.PostSupports.Add(new PostSupport
            {
                PostId = id,
                UserId = userId
            });
        }

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeletePostAsync(int id)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (post.UserId != userId)
        {
            return Forbid();
        }

        _context.Posts.Remove(post);

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string ProfileImage { get; set; } = "";

        public int FormattedSupportCount { get; set; }
        public string UserId { get; set; } = "";
        public bool IsSupportedByCurrentUser { get; set; }
        public int CommentCount { get; set; }

        public string TimeAgo
        {
            get
            {
                var span = DateTime.UtcNow - CreatedAt;

                if (span.TotalSeconds < 60)
                    return $"{(int)span.TotalSeconds}s";

                if (span.TotalMinutes < 60)
                    return $"{(int)span.TotalMinutes}m";

                if (span.TotalHours < 24)
                    return $"{(int)span.TotalHours}h";

                if (span.TotalDays < 30)
                    return $"{(int)span.TotalDays}d";

                if (span.TotalDays < 365)
                    return $"{(int)(span.TotalDays / 30)}mo";

                return $"{(int)(span.TotalDays / 365)}y";
            }
        }
    }
}