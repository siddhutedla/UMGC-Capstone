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

                // Ensure the stream is at the beginning before deserializing
                memoryStream.Position = 0;
                
                var savedConcert = await JsonSerializer.DeserializeAsync<SavedConcert>(memoryStream);
                if (savedConcert == null)
                {
                    _logger.LogError("Deserialization failed; received null SavedConcert object");
                    return BadRequest("Invalid concert data.");
                }

                // Log the deserialized concert to confirm data is correct
                _logger.LogInformation("Deserialized concert data: {SavedConcert}", JsonSerializer.Serialize(savedConcert));

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
    }
}
