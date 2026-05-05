using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webChat.Models;

namespace webChat.Pages.Posts;

public class IndexModel : PageModel
{
    public List<Post> Posts { get; set; } = new();

    public async Task OnGetAsync()
    {
        using var client = new HttpClient();

        var response = await client.GetStringAsync("https://localhost:7202/api/posts");

        Posts = JsonSerializer.Deserialize<List<Post>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Post>();
    }
}