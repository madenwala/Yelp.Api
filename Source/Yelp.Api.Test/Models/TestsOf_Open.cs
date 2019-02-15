using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yelp.Api.Models;

namespace Yelp.Api.Test.Models
{
    [TestClass]
    public class TestsOf_Open
    {
        [TestMethod]
        public void ToString_HoursAreOnMonday_CorrectDayIsReturned()
        {
            var open = new Open
            {
                End = "2000",
                IsOvernight = false,
                Day = 0,
                Start = "0900"
            };

            string result = open.ToString();

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.AreEqual("Monday: 9:00AM - 8:00PM", result);
        }

        [TestMethod]
        public void ToString_HoursAreOnSunday_CorrectDayIsReturned()
        {
            var open = new Open
            {
                End = "2000",
                IsOvernight = false,
                Day = 6,
                Start = "0900"
            };

            string result = open.ToString();

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.AreEqual("Sunday: 9:00AM - 8:00PM", result);
        }
    }
}
