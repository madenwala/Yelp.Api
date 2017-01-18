using Newtonsoft.Json;

namespace Yelp.Api.Model
{
    public class Region
    {
        [JsonProperty("center")]
        public Coordinates Center { get; set; }
    }
}