using System.ComponentModel.DataAnnotations;

namespace webChat.Models
{
    public class PostLike
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public int PostId { get; set; }
        
        public bool IsUpvote { get; set; }
    }
}