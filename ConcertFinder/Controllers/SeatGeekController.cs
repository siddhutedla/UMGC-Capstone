using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using ConcertFinder.Configuration;

namespace ConcertFinder.Controllers
{
    public class SeatGeekController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly SeatGeekSettings _seatGeekSettings;

        public SeatGeekController(IHttpClientFactory clientFactory, IOptions<SeatGeekSettings> seatGeekSettings)
        {
            _clientFactory = clientFactory;
            _seatGeekSettings = seatGeekSettings.Value;
        }

        [HttpGet("/search")]
        public async Task<IActionResult> Search(string artist)
        {
            if (string.IsNullOrEmpty(artist))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Artist name is required");
            }

            var client = _clientFactory.CreateClient("SeatGeekClient");
            var response = await client.GetAsync($"events?q={Uri.EscapeDataString(artist)}&client_id={_seatGeekSettings.ClientId}&client_secret={_seatGeekSettings.ClientSecret}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to retrieve events");
            }
        }
    }
}
