using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Yelp.Api.Models;

namespace Yelp.Api.Test
{
    [TestClass]
    public class UnitTest1
    {
        #region Variables

        private const string API_KEY = "DIPv31nNF1wnUg7VdqZEqU4GDg17kTlB8GzuwF9RXsWbo8yVXcRrEd5r_9G2oTjN8eooq5umIHJM7sAt0m1eVDY6lVTnQAlyzBGVNQ6SqL9f-ezPMOsLriVI4jEvWXYx";

        private readonly Client _client;

        #endregion

        #region Constructors

        public UnitTest1()
        {
            _client = new Client(API_KEY);
        }

        #endregion

        #region Methods

        [TestMethod]
        public void TestSearch()
        {
            var response = _client.SearchBusinessesAllAsync("cupcakes", 37.786882, -122.399972).Result;

            Assert.AreNotSame(null, response);
            Assert.AreSame(null, response?.Error, $"Response error returned {response?.Error?.Code} - {response?.Error?.Description}");
        }

        [TestMethod]
        public void TestSearchDelivery()
        {
            var response = _client.SearchBusinessesWithDeliveryAsync("mex", 37.786882, -122.399972).Result;

            Assert.AreNotSame(null, response);
            Assert.AreSame(null, response?.Error, $"Response error returned {response?.Error?.Code} - {response?.Error?.Description}");
        }

        [TestMethod]
        public void TestAutocomplete()
        {
            var response = _client.AutocompleteAsync("hot dogs", 37.786882, -122.399972).Result;

            Assert.IsTrue(response.Categories.Length > 0);
            Assert.AreNotSame(null, response);
            Assert.AreSame(null, response?.Error, $"Response error returned {response?.Error?.Code} - {response?.Error?.Description}");
        }

        [TestMethod]
        public void TestGetBusiness()
        {
            var response = _client.GetBusinessAsync("north-india-restaurant-san-francisco").Result;

            Assert.AreNotSame(null, response);
            Assert.AreSame(null, response?.Error, $"Response error returned {response?.Error?.Code} - {response?.Error?.Description}");
        }

        [TestMethod]
        public void TestGetBusiness_RequestAlias_AliasComesBack()
        {
            var response = _client.GetBusinessAsync("north-india-restaurant-san-francisco").Result;

            Assert.AreNotSame(null, response);
            string id = response.Id;
            string alias = response.Alias;
            Assert.IsTrue(!string.IsNullOrEmpty(id), $"Id returned null or empty.");
            Assert.IsTrue(!string.IsNullOrEmpty(alias), $"Alias returned null or empty.");
            Assert.IsFalse(id.Equals(alias), $"Alias and Id are the same.");
        }

        [TestMethod]
        public void TestGetBusinessAsyncInParallel()
        {
            List<string> businessIds = new List<string> { "north-india-restaurant-san-francisco" };

            var response = _client.GetBusinessAsyncInParallel(businessIds).Result;

            Assert.AreNotSame(null, response);
            Assert.AreSame(null, response?.FirstOrDefault().Error, $"Response error returned {response?.FirstOrDefault().Error?.Code} - {response?.FirstOrDefault().Error?.Description}");
        }

        [TestMethod]
        public void TestGetReviews()
        {
            var response = _client.GetReviewsAsync("north-india-restaurant-san-francisco").Result;

            Assert.AreNotSame(null, response);
            Assert.AreSame(null, response?.Error, $"Response error returned {response?.Error?.Code} - {response?.Error?.Description}");
        }



        [TestMethod]
        public void TestGetModelChanges()
        {
            var m = new SearchRequest();
            m.Term = "Hello world";
            m.Price = "$";
            var dic = m.GetChangedProperties();

            Assert.AreEqual(dic.Count, 2);
            Assert.IsTrue(dic.ContainsKey("term"));
            Assert.IsTrue(dic.ContainsKey("price"));
        }

        #endregion

        #region GraphQL Methods *REQUIRES APP TO BE IN BETA*

        [TestMethod]
        public void TestGetGraphQlAsync()
        {
            List<string> businessIds = new List<string> { "north-india-restaurant-san-francisco" };

            var response = _client.GetGraphQlAsync(businessIds).Result;

            Assert.AreNotSame(null, response);
            Assert.AreSame(null, response?.FirstOrDefault().Error, $"Response error returned {response?.FirstOrDefault().Error?.Code} - {response?.FirstOrDefault().Error?.Description}");
        }

        [TestMethod]
        public void TestGetGraphQlAsync_RequestAlias_AliasComesBack()
        {
            List<string> businessIds = new List<string> { "north-india-restaurant-san-francisco" };

            var response = _client.GetGraphQlAsync(businessIds).Result;

            Assert.AreNotSame(null, response);
            string id = response.FirstOrDefault().Id;
            string alias = response.FirstOrDefault().Alias;
            Assert.IsTrue(!string.IsNullOrEmpty(id), $"Id returned null or empty.");
            Assert.IsTrue(!string.IsNullOrEmpty(alias), $"Alias returned null or empty.");
            Assert.IsFalse(id.Equals(alias), $"Alias and Id are the same.");
        }

        [TestMethod]
        public void TestGetGraphQlInChunksAsync()
        {
            List<string> businessIds = new List<string> { "north-india-restaurant-san-francisco" };

            var response = _client.GetGraphQlInChunksAsync(businessIds).Result;

            Assert.AreNotSame(null, response);
            Assert.AreSame(null, response?.FirstOrDefault().Error, $"Response error returned {response?.FirstOrDefault().Error?.Code} - {response?.FirstOrDefault().Error?.Description}");
        }

        [TestMethod]
        public void TestGetGraphQlInChunksAsyncInParallel()
        {
            List<string> businessIds = new List<string> { "north-india-restaurant-san-francisco" };

            var response = _client.GetGraphQlInChunksAsyncInParallel(businessIds);

            Assert.AreNotSame(null, response);
            Assert.AreSame(null, response.Result.FirstOrDefault()?.Error,
                $"Response error returned {response.Result.FirstOrDefault()?.Error?.Code} - {response.Result.FirstOrDefault()?.Error?.Description}");
        }

        #endregion
    }
}
