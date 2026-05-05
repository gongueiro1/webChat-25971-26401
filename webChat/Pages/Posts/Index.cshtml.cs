using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace webChat.Pages.Posts;

public class IndexModel : PageModel
{
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