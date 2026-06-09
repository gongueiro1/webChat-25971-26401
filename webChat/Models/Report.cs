namespace webChat.Models;

public class Report
{
    public int Id { get; set; }

    public int PostId { get; set; }
    public Post? Post { get; set; }

    public string ReporterUserId { get; set; } = string.Empty;
    public ApplicationUser? ReporterUser { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Aberta";
}