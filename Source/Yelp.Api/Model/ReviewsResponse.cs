using Newtonsoft.Json;

namespace Yelp.Api.Model
{
    public class ReviewsResponse : ResponseBase
    {
        [JsonProperty("reviews")]
        public Review[] Reviews { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}