using Newtonsoft.Json;
using Yelp.Api.Helpers;

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

        public override string ToString()
        {
            // Yelp represents days of the week as 0-6 with 0 being Monday.
            // C# represents days of the week as 0-6 with 0 being Sunday.
            // So to convert to System.DayOfWeek we will need to add one and then modulus 7.
            int dayOffset = (Day + 1) % 7;

            return $"{(System.DayOfWeek)dayOffset}: {TimeHelper.FormatTime(Start)} - {TimeHelper.FormatTime(End)}";
        }
    }
}