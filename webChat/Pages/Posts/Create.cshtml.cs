using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims; // Obrigatório para ir buscar o ID do utilizador
using webChat.Data;
using webChat.Models;

namespace webChat.Pages.Posts
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            // Opcional mas recomendado: Se o gajo não estiver logado, manda-o para o Login!
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            return Page();
        }

        [BindProperty]
        public Post Post { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Ignorar os erros do UserId e AuthorName porque nós é que os vamos preencher agora
            ModelState.Remove("Post.UserId");
            ModelState.Remove("Post.AuthorName");

            if (!ModelState.IsValid || _context.Posts == null || Post == null)
            {
                return Page();
            }

            // 2. A MÁGICA: Preencher quem é o dono do Post automaticamente!
            Post.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            Post.AuthorName = User.Identity?.Name ?? "Anónimo";
            Post.CreatedAt = DateTime.UtcNow;

            // 3. Guardar na Base de Dados
            _context.Posts.Add(Post);
            await _context.SaveChangesAsync();

            // 4. Voltar para o Feed
            return RedirectToPage("./Index");
        }
    }
}