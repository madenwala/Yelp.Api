using Newtonsoft.Json;

namespace Yelp.Api.Model
{
    public class Business
    {
        [JsonProperty("categories")]
        public Category[] Categories { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("rating")]
        public float Rating { get; set; }

        [JsonProperty("coordinates")]
        public Coordinates Coordinates { get; set; }

        [JsonProperty("display_phone")]
        public string DisplayPhone { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("distance")]
        public float Distance { get; set; }

        [JsonProperty("is_closed")]
        public bool IsClosed { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("review_count")]
        public int ReviewCount { get; set; }
    }
}