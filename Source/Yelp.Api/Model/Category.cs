using Newtonsoft.Json;

namespace Yelp.Api.Model
{
    public class Category
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }
    }
}