using ConcertFinder.Data;
using ConcertFinder.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            var passwordInput = form["password"].FirstOrDefault();

            if (string.IsNullOrEmpty(passwordInput))
            {
                return BadRequest("Password cannot be empty");
            }
            if (username == null)
            {
                return BadRequest("Username cannot be empty");
            }
            var password = HashPassword(passwordInput);
            if (await _dbContext.Users.AnyAsync(u => u.Username == username))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Username already exists");
            }
            var newUser = new User { Username = username, Password = password };
            await _dbContext.Users.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();
            return Redirect("/login");
        }

        // Login
        [HttpPost("/login")]
        public async Task<IActionResult> Login(IFormCollection form)
        {
            var username = form["username"].FirstOrDefault();
            var passwordInput = form["password"].FirstOrDefault();

            if (string.IsNullOrEmpty(passwordInput))
            {
                return BadRequest("Password cannot be empty");
            }

            var password = HashPassword(passwordInput);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid username or password");
            }

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.Username);

            return Redirect("/");
        }

        // Logout
        [HttpPost("/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/login");
        }

        // Change Password
        [HttpPost("/change-password")]
        public async Task<IActionResult> ChangePassword(IFormCollection form)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, "User not logged in");
            }

            var currentPasswordInput = form["currentPassword"].FirstOrDefault();
            var newPasswordInput = form["newPassword"].FirstOrDefault();
            var confirmPasswordInput = form["confirmPassword"].FirstOrDefault();

            if (string.IsNullOrEmpty(currentPasswordInput) || string.IsNullOrEmpty(newPasswordInput) || string.IsNullOrEmpty(confirmPasswordInput))
            {
                return BadRequest("Passwords cannot be empty");
            }

            var currentPassword = HashPassword(currentPasswordInput);
            var newPassword = HashPassword(newPasswordInput);
            var confirmPassword = HashPassword(confirmPasswordInput);

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

        // Hash passwords
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
