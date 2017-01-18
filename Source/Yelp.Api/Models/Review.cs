using Newtonsoft.Json;

namespace Yelp.Api.Models
{
    public class Review
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("time_created")]
        public string TimeCreated { get; set; }
    }
}