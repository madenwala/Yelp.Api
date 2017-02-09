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

        private string FormatTime(string time)
        {
            try
            {
                var dt = System.DateTime.Today.AddHours(double.Parse(time.Substring(0, 2))).AddMinutes(double.Parse(time.Substring(2)));
                return dt.ToString("h:mmtt").ToLowerInvariant();
            }
            catch
            {
                return time;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} - {2}", (System.DayOfWeek)this.Day, this.FormatTime(this.Start), this.FormatTime(this.End));
        }
    }
}