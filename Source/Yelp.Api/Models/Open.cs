using Newtonsoft.Json;

namespace Yelp.Api.Models
{
    public class Open
    {
        [JsonProperty("end")]
        public string End { get; set; }

        [JsonProperty("is_overnight")]
        public bool IsOvernight { get; set; }

        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }
    }
}