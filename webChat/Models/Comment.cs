using System.ComponentModel.DataAnnotations;

namespace webChat.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    [StringLength(1000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public string? ProfileImage { get; set; }

    public int PostId { get; set; }

    public Post? Post { get; set; }
    
    public int? ParentCommentId { get; set; }

    public Comment? ParentComment { get; set; }

    public List<Comment> Replies { get; set; } = new();

    public string? ImageUrl { get; set; }
}