using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ConcertFinder.Controllers
{
    public class HomeController : Controller
    {

        [HttpGet("/api/isLoggedIn")]
        public IActionResult IsLoggedIn()
        {
            bool isLoggedIn = HttpContext.Session.GetString("UserId") != null;
            return Ok(new { IsLoggedIn = isLoggedIn });
        }

        [HttpGet("/get-username")]
        public IActionResult GetUsername()
        {
            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(username))
            {
                return Ok(new { username = username });
            }
            else
            {
                return Unauthorized("User is not logged in.");
            }
        }

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
        public IActionResult Login()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "view", "login.html");
            return PhysicalFile(filePath, "text/html");
        }

        [HttpGet("/register")]
        public IActionResult Register()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "view", "register.html");
            return PhysicalFile(filePath, "text/html");
        }

        [HttpGet("/saved")]
        public IActionResult Pins()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "view", "saved.html");
            return PhysicalFile(filePath, "text/html");
        }


        [HttpGet("/account")]
        public IActionResult AccountSettings()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return Redirect("/login");
            }
            else
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "view", "account.html");
                return PhysicalFile(filePath, "text/html");
            }
        }
    }
}