using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using ConcertFinder.Configuration;
using Microsoft.Extensions.Logging;

namespace ConcertFinder.Controllers
{
    public class SeatGeekController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly SeatGeekSettings _seatGeekSettings;
        private readonly ILogger<SeatGeekController> _logger;

        public SeatGeekController(IHttpClientFactory clientFactory, IOptions<SeatGeekSettings> seatGeekSettings, ILogger<SeatGeekController> logger)
        {
            _clientFactory = clientFactory;
            _seatGeekSettings = seatGeekSettings.Value;
            _logger = logger;
        }

        [HttpGet("/search")]
        public async Task<IActionResult> Search(string artist, int page = 1)
        {
            if (string.IsNullOrEmpty(artist))
            {
                _logger.LogWarning("Search attempted without artist name.");
                return StatusCode(StatusCodes.Status400BadRequest, "Artist name is required");
            }

            var client = _clientFactory.CreateClient("SeatGeekClient");
            var url = $"events?q={Uri.EscapeDataString(artist)}&client_id={_seatGeekSettings.ClientId}&client_secret={_seatGeekSettings.ClientSecret}&page={page}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            else
            {
                _logger.LogError($"Failed to retrieve events for artist {artist}. Status code: {response.StatusCode}.");
                return StatusCode((int)response.StatusCode, "Failed to retrieve events");
            }
        }
    }
}
