using System;
using System.Text.Json.Serialization;

public class SavedConcert
{
    public int Id { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("performers")]
    public string Performers { get; set; } = string.Empty;

    [JsonPropertyName("venueName")]
    public string VenueName { get; set; } = string.Empty;

    [JsonPropertyName("venueCity")]
    public string VenueCity { get; set; } = string.Empty;

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("dateTime")]
    public DateTime DateTime { get; set; }

    [JsonPropertyName("eventUrl")]
    public string EventUrl { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public double Score { get; set; } = 0.0;


}



