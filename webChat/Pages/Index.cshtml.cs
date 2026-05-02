using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace webChat.Pages;

public class IndexModel : PageModel {
    public IActionResult OnGet()
    {
        return RedirectToPage("/Posts/Index");
    }
}