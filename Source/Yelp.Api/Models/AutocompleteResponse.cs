using Newtonsoft.Json;

namespace Yelp.Api.Models
{
    public class AutocompleteResponse : ResponseBase
    {
        [JsonProperty("businesses")]
        public BusinessResponse[] Businesses { get; set; }

        [JsonProperty("terms")]
        public Term[] Terms { get; set; }

        [JsonProperty("categories")]
        public Category[] Categories { get; set; }
    }

    public class Term
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}