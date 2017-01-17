using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Yelp.Api.Test
{
    [TestClass]
    public class UnitTest1
    {
        private const string APP_ID = "39ukJIrbqD1Pk5V16B5weA";
        private const string APP_SECRET = "pDgLtCkYCRAnTpI9TP15xRMV4yeX75UMud05z7Cksm0KuCpw5qpQLMfVWzmOSBKC";

        [TestMethod]
        public void TestAuthToken()
        {
            Client client = new Client(APP_ID, APP_SECRET);

            var response = client.GetTokenAsync(CancellationToken.None).Result;

            Assert.AreNotSame(null, response);
            Assert.AreSame(null, response?.Error);
        }

        [TestMethod]
        public void TestSearch()
        {
            Client client = new Client(APP_ID, APP_SECRET);

            var response = client.SearchBusinessesAsync("cupcakes", 37.786882, -122.399972, CancellationToken.None).Result;

            Assert.AreNotSame(null, response);
            //Assert.AreSame(null, response?.Error);
        }
    }
}