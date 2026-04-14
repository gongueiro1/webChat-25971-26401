using System.ComponentModel.DataAnnotations;

namespace webChat.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    [StringLength(1000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string? UserId { get; set; }

    public int PostId { get; set; }
    public Post? Post { get; set; }
}