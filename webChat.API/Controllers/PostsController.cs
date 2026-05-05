using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webChat.Data;

namespace webChat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PostsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPosts()
    {
        var posts = await _context.Posts
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Content,
                p.AuthorName,
                p.CreatedAt,

                // imagem temporária
                ProfileImage = "/images/avatars/default-avatar.png"
            })
            .ToListAsync();

        return Ok(posts);
    }
}