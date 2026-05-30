using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webChat.Data; // <-- Preciso para aceder à Base de Dados
using webChat.Models; // <-- Preciso para aceder à tabela PostSupport

namespace webChat.Pages.Posts;

public class IndexModel : PageModel
{
    // 1. Ligar o "cérebro" da página à Base de Dados
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

    // --- A MAGIA DO BOTÃO DE SUPPORT COMEÇA AQUI ---
    public async Task<IActionResult> OnPostSupportAsync(int id)
    {
        // Descobre quem é o utilizador que está a clicar no botão
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        // Se for um "fantasma" (sem login feito), manda-o para a página de Login
        if (userId == null) 
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        // Vai à base de dados ver se ESTE utilizador já deu Support a ESTE post
        var existingSupport = _context.PostSupports
            .FirstOrDefault(s => s.PostId == id && s.UserId == userId);

        if (existingSupport != null)
        {
            // Se já existia, significa que ele quer tirar o like (Unlike)
            _context.PostSupports.Remove(existingSupport);
        }
        else
        {
            // Se não existia, cria um like novo!
            _context.PostSupports.Add(new PostSupport 
            { 
                PostId = id, 
                UserId = userId 
            });
        }

        // Guarda as alterações na base de dados
        await _context.SaveChangesAsync();

        // Faz refresh à página para mostrar o novo número de Supports
        return RedirectToPage();
    }
    // -----------------------------------------------

    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string ProfileImage { get; set; } = "";
        
        // Estas variáveis precisam de existir para o HTML as conseguir ler
        public int FormattedSupportCount { get; set; }
        public string UserId { get; set; } 
    }
}