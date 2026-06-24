namespace webChat.Models;

public class PostSupport
{
    public string UserId { get; set; } = string.Empty;
    public int PostId { get; set; }

    public ApplicationUser? User { get; set; }
    public Post? Post { get; set; }
}