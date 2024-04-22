using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ConcertFinder.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/")]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return Redirect("/login");
            }
            else
            {
                var username = HttpContext.Session.GetString("Username");
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "view", "home.html");
                var htmlContent = await System.IO.File.ReadAllTextAsync(filePath);
                htmlContent = htmlContent.Replace("[default text or empty]", username);
                return Content(htmlContent, "text/html", Encoding.UTF8);
            }
        }

        [HttpGet("/login")]
        public async Task<IActionResult> Login()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "view", "login.html");
            return PhysicalFile(filePath, "text/html");
        }

        [HttpGet("/register")]
        public async Task<IActionResult> Register()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "view", "register.html");
            return PhysicalFile(filePath, "text/html");
        }

        [HttpGet("/account-settings")]
        public async Task<IActionResult> AccountSettings()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return Redirect("/login");
            }
            else
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "view", "account-settings.html");
                return PhysicalFile(filePath, "text/html");
            }
        }
    }
}
