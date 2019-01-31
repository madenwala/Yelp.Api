using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yelp.Api.Models;

namespace Yelp.Api.Test.Models
{
    [TestClass]
    public class TestsOf_SpecialHour
    {
        [TestMethod]
        public void ToString_BusinessIsOpen_ReturnsCorrectString()
        {
            var specialHour = new SpecialHour
            {
                Date = DateTime.Today,
                End = "2000",
                IsClosed = false,
                IsOvernight = false,
                Start = "0900"
            };

            string result = specialHour.ToString();

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.AreEqual($"{DateTime.Today:MM/dd/yyyy}: 9:00AM - 8:00PM", result);
        }

        [TestMethod]
        public void ToString_BusinessIsClosed_ReturnsCorrectString()
        {
            var specialHour = new SpecialHour
            {
                Date = DateTime.Today,
                End = null,
                IsClosed = true,
                IsOvernight = false,
                Start = null
            };

            string result = specialHour.ToString();

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.AreEqual($"{DateTime.Today:MM/dd/yyyy}: CLOSED", result);
        }
    }
}
