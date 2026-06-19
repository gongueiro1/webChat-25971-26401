using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace webChat.Pages.Posts;

public class IndexModel : PageModel
{
    public List<PostDto> Posts { get; set; } = new();

    public async Task OnGetAsync()
    {
        // O facto de estares a chamar a API significa que, se a API 
        // devolver todos os posts, o teu feed já é global automaticamente!
        using var client = new HttpClient();
        
        var response = await client.GetStringAsync("http://localhost:5030/api/posts");

        Posts = JsonSerializer.Deserialize<List<PostDto>>(response,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<PostDto>();
    }

    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string ProfileImage { get; set; } = "";
    }
}