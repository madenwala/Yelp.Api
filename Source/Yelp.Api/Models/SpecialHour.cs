using System;
using Newtonsoft.Json;
using Yelp.Api.Helpers;

namespace Yelp.Api.Models
{
    public class SpecialHour
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }

        [JsonProperty("is_closed")]
        public bool IsClosed { get; set; }

        [JsonProperty("is_overnight")]
        public bool IsOvernight { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }
        
        public override string ToString()
        {
            string output = IsClosed ? 
                $"{Date:MM/dd/yyyy}: CLOSED" : 
                $"{Date:MM/dd/yyyy}: {TimeHelper.FormatTime(Start)} - {TimeHelper.FormatTime(End)}";

            return output;
        }
    }
}
