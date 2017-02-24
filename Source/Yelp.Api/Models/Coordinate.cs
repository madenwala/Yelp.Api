using Newtonsoft.Json;

namespace Yelp.Api.Models
{
    public interface ICoordinates
    {
        double Latitude { get; set; }
        double Longitude { get; set; }
    }

    public class Coordinates : ICoordinates
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