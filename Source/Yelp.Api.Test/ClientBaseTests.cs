using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yelp.Api.Test
{
    [TestClass]
    public class ClientBaseTests
    {
        #region Variables

        private const string API_KEY = "DIPv31nNF1wnUg7VdqZEqU4GDg17kTlB8GzuwF9RXsWbo8yVXcRrEd5r_9G2oTjN8eooq5umIHJM7sAt0m1eVDY6lVTnQAlyzBGVNQ6SqL9f-ezPMOsLriVI4jEvWXYx";

        private readonly Client _client;

        #endregion

        #region Constructors

        public ClientBaseTests()
        {
            _client = new Client(API_KEY);
        }

        #endregion

        [TestMethod]
        public void GetAsync()
        {
            // TODO: Write a test that makes sure null coordinates don't crash
            // TODO: Write a test that makes sure null cordinates are set to NaN
        }

        [TestMethod]
        public void PostAsync()
        {
            // TODO: Write a test that makes sure null coordinates don't crash
            // TODO: Write a test that makes sure null cordinates are set to NaN
        }
    }
}