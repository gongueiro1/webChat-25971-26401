using System.ComponentModel.DataAnnotations;

namespace webChat.Models;

public class ChatMessage
{
    public int Id { get; set; }

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.Now;

    [Required]
    public string SenderId { get; set; } = string.Empty;

    [Required]
    public string ReceiverId { get; set; } = string.Empty;
}