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
    
    // Variável para guardar o que o utilizador pesquisou
    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    // Repara que adicionei o parâmetro searchString aqui!
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

        // --- A MAGIA DA PESQUISA ACONTECE AQUI ---
        if (!string.IsNullOrEmpty(SearchString))
        {
            // Filtra a lista mantendo apenas os posts onde o Título ou o Conteúdo têm a palavra pesquisada
            Posts = Posts.Where(p => 
                p.Title.Contains(SearchString, StringComparison.OrdinalIgnoreCase) || 
                p.Content.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
        // -----------------------------------------

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

        // Volta para a mesma página mantendo a pesquisa ativa na barra de endereço
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