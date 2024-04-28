namespace ConcertFinder.Models
{
    public class SavedConcert
    {
        public int Id { get; set; } 
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Performers { get; set; } = string.Empty;
        public string VenueName { get; set; } = string.Empty;
        public string VenueCity { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string EventUrl { get; set; } = string.Empty;
    }
}
