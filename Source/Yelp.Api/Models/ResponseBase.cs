using System;
using Newtonsoft.Json;

namespace Yelp.Api.Models
{
    public abstract class ResponseBase : ModelBase
    {
        [JsonProperty("error")]
        public ResponseError Error { get; set; }

        public RateLimit RateLimit { get; set; }
    }

    public sealed class ResponseError
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public sealed class RateLimit
    {
        public int DailyLimit { get; set; }

        public int Remaining { get; set; }

        public DateTime ResetTime { get; set; }
    }
}