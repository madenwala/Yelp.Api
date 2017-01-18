using Newtonsoft.Json;
using System.Collections.Generic;

namespace Yelp.Api.Models
{
    public class SearchResponse : ResponseBase
    {
        [JsonProperty("region")]
        public Region Region { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("businesses")]
        public IList<BusinessResponse> Businesses { get; set; }
    }
}