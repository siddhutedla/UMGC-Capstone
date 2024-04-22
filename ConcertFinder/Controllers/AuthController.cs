using ConcertFinder.Data;
using ConcertFinder.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ConcertFinder.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _dbContext;

        public AuthController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register(IFormCollection form)
        {
            var username = form["username"].FirstOrDefault();
            var password = form["password"].FirstOrDefault();

            if (await _dbContext.Users.AnyAsync(u => u.Username == username))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Username already exists");
            }

            var newUser = new User { Username = username, Password = password };
            await _dbContext.Users.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();

            return Redirect("/login");
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(IFormCollection form)
        {
            var username = form["username"].FirstOrDefault();
            var password = form["password"].FirstOrDefault();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid username or password");
            }

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.Username);

            return Redirect("/");
        }

        [HttpPost("/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/login");
        }

        [HttpPost("/change-password")]
        public async Task<IActionResult> ChangePassword(IFormCollection form)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, "User not logged in");
            }

            var currentPassword = form["currentPassword"].FirstOrDefault();
            var newPassword = form["newPassword"].FirstOrDefault();
            var confirmPassword = form["confirmPassword"].FirstOrDefault();

            var user = await _dbContext.Users.FindAsync(int.Parse(userId));

            if (user == null || user.Password != currentPassword)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Current password is incorrect or user not found");
            }

            if (newPassword != confirmPassword)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "New passwords do not match");
            }

            user.Password = newPassword;
            await _dbContext.SaveChangesAsync();

            return Ok("Password updated successfully");
        }
    }
}
