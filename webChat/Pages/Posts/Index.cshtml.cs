using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using webChat.Data;

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

        var response = await client.GetStringAsync("https://localhost:7202/api/posts");

        Posts = JsonSerializer.Deserialize<List<PostDto>>(response,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<PostDto>();
    }

    public async Task<IActionResult> OnPostSupportAsync(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        using var client = new HttpClient();

        await client.PostAsync(
            $"https://localhost:7202/api/posts/{id}/support?userId={userId}",
            null);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeletePostAsync(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var post = await _context.Posts.FindAsync(id);

        if (post == null)
        {
            return NotFound();
        }

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

        public int SupportCount { get; set; }

        public string UserId { get; set; } = "";

        public string FormattedSupportCount =>
            SupportCount >= 1000000 ? $"{SupportCount / 1000000.0:0.#}M" :
            SupportCount >= 1000 ? $"{SupportCount / 1000.0:0.#}k" :
            SupportCount.ToString();
    }
}