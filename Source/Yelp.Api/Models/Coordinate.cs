using Newtonsoft.Json;

namespace Yelp.Api.Models
{
    public class Coordinates
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }
}