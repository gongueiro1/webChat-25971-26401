using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR; // <-- Importante
using webChat.Data;
using webChat.Hubs; // <-- Importante
using webChat.Models;

namespace webChat.Pages.Posts
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext; // <-- Cérebro do SignalR

        public CreateModel(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public IActionResult OnGet()
        {
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
            ModelState.Remove("Post.UserId");
            ModelState.Remove("Post.AuthorName");

            if (!ModelState.IsValid || _context.Posts == null || Post == null)
            {
                return Page();
            }

            Post.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            Post.AuthorName = User.Identity?.Name ?? "Anónimo";
            Post.CreatedAt = DateTime.UtcNow;

            _context.Posts.Add(Post);
            await _context.SaveChangesAsync();

            // --- ALARME PARA O TEU TERMINAL DO RIDER ---
            Console.WriteLine($"\n\n ---> [SIGNALR] A enviar aviso de novo post do autor: {Post.AuthorName} <--- \n\n");

            // --- MAGIA SIGNALR ---
            await _hubContext.Clients.All.SendAsync("ReceiveNewPost", new { 
                author = Post.AuthorName 
            });

            return RedirectToPage("./Index");
        }
    }
}