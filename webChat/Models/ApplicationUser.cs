using Microsoft.AspNetCore.Identity;

namespace webChat.Models;

public class ApplicationUser : IdentityUser
{
    public string? ProfileImageUrl { get; set; }
}