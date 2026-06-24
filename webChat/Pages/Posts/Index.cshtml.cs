using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    
    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    public async Task OnGetAsync(string? searchString)
    {
        SearchString = searchString;

        using var client = new HttpClient();
        var response = await client.GetStringAsync("https://localhost:7202/api/posts");
        
        Posts = JsonSerializer.Deserialize<List<PostDto>>(response,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<PostDto>();

        // Filtro da Barra de Pesquisa
        if (!string.IsNullOrEmpty(SearchString))
        {
            Posts = Posts.Where(p => 
                p.Title.Contains(SearchString, StringComparison.OrdinalIgnoreCase) || 
                p.Content.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        if (Posts.Any())
        {
            var postIds = Posts.Select(p => p.Id).ToList();
            
            var allSupports = _context.PostSupports
                .Where(s => postIds.Contains(s.PostId))
                .ToList();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            foreach (var post in Posts)
            {
                post.FormattedSupportCount = allSupports.Count(s => s.PostId == post.Id);
                post.IsSupportedByCurrentUser = userId != null && allSupports.Any(s => s.PostId == post.Id && s.UserId == userId);
                
                // --- A MAGIA DO TEMPO COMEÇA AQUI ---
                var ts = DateTime.UtcNow - post.CreatedAt;
                if (ts.TotalSeconds < 60) 
                    post.TimeAgo = $"{(int)ts.TotalSeconds}s";
                else if (ts.TotalMinutes < 60) 
                    post.TimeAgo = $"{(int)ts.TotalMinutes}m";
                else if (ts.TotalHours < 24) 
                    post.TimeAgo = $"{(int)ts.TotalHours}h";
                else if (ts.TotalDays < 30) 
                    post.TimeAgo = $"{(int)ts.TotalDays}d";
                else if (ts.TotalDays < 365) 
                    post.TimeAgo = $"{(int)(ts.TotalDays / 30)}M"; // 'M' maiúsculo para não confundir com minutos
                else 
                    post.TimeAgo = $"{(int)(ts.TotalDays / 365)}a";
                // ------------------------------------
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
            _context.PostSupports.Add(new PostSupport { PostId = id, UserId = userId });
        }

        await _context.SaveChangesAsync();
        return RedirectToPage(new { searchString = Request.Query["searchString"] });
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
        public string UserId { get; set; } 
        public bool IsSupportedByCurrentUser { get; set; } 
        public string TimeAgo { get; set; } = "";
        public int CommentCount { get; set; }
    }
}