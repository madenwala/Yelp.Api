using Newtonsoft.Json;

namespace Yelp.Api.Models
{
    public class Region
    {
        [JsonProperty("center")]
        public Coordinates Center { get; set; }
    }
}