using Newtonsoft.Json;

namespace Yelp.Api.Models
{
    public class Coordinates
    {
        public Coordinates()
        {
            this.Latitude = double.MinValue;
            this.Longitude = double.MinValue;
        }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }
}