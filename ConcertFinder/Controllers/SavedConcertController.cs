using ConcertFinder.Data;
using ConcertFinder.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ConcertFinder.Controllers
{
    public class SavedConcertController : Controller
    {
        private readonly AppDbContext _context;

        public SavedConcertController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("/api/save-concert")]
        public async Task<IActionResult> SaveConcert([FromBody] SavedConcert savedConcert)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User must be logged in to save concerts.");
            }
            savedConcert.UserId = userId;

            // Check if the concert is already saved to prevent duplicates
            var exists = await _context.SavedConcerts.AnyAsync(c => c.EventUrl == savedConcert.EventUrl && c.UserId == userId);
            if (exists)
            {
                return BadRequest("Concert is already saved.");
            }

            _context.SavedConcerts.Add(savedConcert);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Concert saved successfully" });
        }

        [HttpGet("/api/saved-concerts")]
        public async Task<IActionResult> GetSavedConcerts()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User must be logged in to view saved concerts.");
            }

            var savedConcerts = await _context.SavedConcerts
                                              .Where(sc => sc.UserId == userId)
                                              .ToListAsync();

            return Ok(savedConcerts);
        }

        [HttpDelete("/api/remove-saved-concert/{id}")]
        public async Task<IActionResult> RemoveSavedConcert(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User must be logged in to remove concerts.");
            }

            var savedConcert = await _context.SavedConcerts
                                             .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (savedConcert == null)
            {
                return NotFound("Concert not found.");
            }

            _context.SavedConcerts.Remove(savedConcert);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Concert removed successfully" });
        }
    }
}
