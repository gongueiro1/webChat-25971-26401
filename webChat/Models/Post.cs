using System.ComponentModel.DataAnnotations;

namespace webChat.Models;

public class Post
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;

    public string AuthorName { get; set; } = string.Empty;
}