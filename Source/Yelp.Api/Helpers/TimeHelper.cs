using System.Globalization;

namespace Yelp.Api.Helpers
{
    public static class TimeHelper
    {
        public static string FormatTime(string time)
        {
            try
            {
                var dt = System.DateTime.Today.AddHours(double.Parse(time.Substring(0, 2))).AddMinutes(double.Parse(time.Substring(2)));
                return dt.ToString("h:mmtt", CultureInfo.InvariantCulture);
            }
            catch
            {
                return time;
            }
        }
    }
}
