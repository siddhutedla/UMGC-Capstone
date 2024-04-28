using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ConcertFinder.Data;
using ConcertFinder.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Linq;

namespace ConcertFinder.Controllers
{
    public class SavedConcertController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SavedConcertController> _logger;

        public SavedConcertController(AppDbContext context, ILogger<SavedConcertController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("/api/save-concert")]
        public async Task<IActionResult> SaveConcert()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                _logger.LogInformation("Received body: {RequestBody}", body);

                var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(body));
                memoryStream.Position = 0;  
                
                var savedConcert = await JsonSerializer.DeserializeAsync<SavedConcert>(memoryStream);
                if (savedConcert == null)
                {
                    _logger.LogError("Deserialization failed; received null SavedConcert object");
                    return BadRequest("Invalid concert data.");
                }

                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized attempt to save a concert");
                    return Unauthorized("User must be logged in to save concerts.");
                }

                var exists = await _context.SavedConcerts.AnyAsync(c => c.EventUrl == savedConcert.EventUrl && c.UserId == userId);
                if (exists)
                {
                    _logger.LogInformation("Attempt to save duplicate concert: {EventUrl}", savedConcert.EventUrl);
                    return BadRequest("Concert is already saved.");
                }

                savedConcert.UserId = userId;
                try
                {
                    _context.SavedConcerts.Add(savedConcert);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Concert saved successfully: {EventUrl}", savedConcert.EventUrl);
                    return Ok(new { Message = "Concert saved successfully" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving concert");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "An error occurred while saving the concert." });
                }
            }
        }

        // GET: api/saved-concerts
        [HttpGet("/api/saved-concerts")]
        public async Task<IActionResult> GetSavedConcerts()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User must be logged in to view saved concerts.");
            }

            var concerts = await _context.SavedConcerts
                                         .Where(c => c.UserId == userId)
                                         .ToListAsync();

            return Ok(concerts);
        }
        [HttpDelete("/api/remove-saved-concert/{id}")]
        public async Task<IActionResult> RemoveSavedConcert(int id)
        {
            var savedConcert = await _context.SavedConcerts.FindAsync(id);
            if (savedConcert == null)
            {
                return NotFound();
            }

            _context.SavedConcerts.Remove(savedConcert);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

