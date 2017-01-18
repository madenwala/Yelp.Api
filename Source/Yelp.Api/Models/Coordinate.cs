using Newtonsoft.Json;

namespace Yelp.Api.Models
{
    public class Coordinates
    {
        [JsonProperty("latitude")]
        public float Latitude { get; set; }

        [JsonProperty("longitude")]
        public float Longitude { get; set; }
    }
}