using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Yelp.Api;
using Yelp.Api.Models;

namespace ExampleApp.Controllers
{
    public class HomeController : Controller
    {
        private const string _YelpApiKey = "DIPv31nNF1wnUg7VdqZEqU4GDg17kTlB8GzuwF9RXsWbo8yVXcRrEd5r_9G2oTjN8eooq5umIHJM7sAt0m1eVDY6lVTnQAlyzBGVNQ6SqL9f-ezPMOsLriVI4jEvWXYx";

        private readonly Client _client;

        private readonly String[] _yelpIds =
        {
            "de-afghanan-cuisine-fremont", "de-afghanan-kabob-house-fremont-3", "de-afghanan-kabob-house-san-francisco-2", "gulzaar-halal-restaurant-and-catering-san-jose-7", "helmand-palace-san-francisco",
            "kabob-trolley-san-francisco-5", "kabul-afghan-cuisine-san-carlos", "kabul-afghan-cuisine-sunnyvale-2", "kabul-express-kabob-newark", "kabul-kabob-and-grill-dublin",
            "khyber-pass-kabob-dublin", "l-aziz-bakery-eatery-union-city-2", "little-kabul-market-fremont", "maiwand-halal-kabob-truck-san-francisco", "maiwand-kabob-house-fremont",
            "maiwand-kabob-house-santa-clara", "peshawari-kababs-union-city", "qs-halal-chicken-alameda", "rasa-burlingame", "redwood-bistro-redwood-city",
            "rocknwraps-and-kabobs-redwood-city-2", "salang-pass-restaurant-fremont", "shami-restaurant-and-hookah-lounge-san-leandro", "tayyibaat-meat-and-grill-union-city-2", "the-ravioli-house-san-mateo",
            "yakitori-kokko-san-mateo", "zalla-kabab-house-danville", "zam-zam-grill-fremont", "annar-afghan-cuisine-hayward", "izakaya-mai-san-mateo"
        };

        public HomeController()
        {
            _client = new Client(_YelpApiKey);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        #region GetBusiness

        // Test GetBusinessAsync - 30 restaurants retrieved in series, one at a time, in about 8-9 seconds.
        public IActionResult TestGetBusinessAsync()
        {
            List<BusinessResponse> businessResponses = new List<BusinessResponse>();

            foreach (var yelpId in _yelpIds)
            {
                BusinessResponse businessResponse;

                try
                {
                    businessResponse = _client.GetBusinessAsync(
                        yelpId,
                        ct: default(CancellationToken),
                        connectionRetrySettings: new ConnectionRetrySettings()).Result;
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }

                businessResponses.Add(businessResponse);
            }

            return View("Result", businessResponses);
        }

        // Test GetBusinessAsyncInParallel - 30 restaurants retrieved in parallel, one at a time, in about 5-6 seconds.
        public IActionResult TestGetBusinessAsyncInParallel()
        {
            IEnumerable<BusinessResponse> businessResponses;

            try
            {
                businessResponses = _client.GetBusinessAsyncInParallel(
                    _yelpIds.ToList(),
                    ct: default(CancellationToken),
                    maxThreads: 2,
                    connectionRetrySettings: new ConnectionRetrySettings()).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }

            return View("Result", businessResponses);
        }

        #endregion

        #region GraphQL

        // Test GetGraphQlAsync - 30 restaurants retrieved via one GraphQL call in about 3-4 seconds.
        public IActionResult TestGetGraphQlAsync()
        {
            IEnumerable<BusinessResponse> businessResponses;

            try
            {
                // More than 10 items at a time used to crash Yelp's API.  Seems like they fixed it because now I can send at least 30 at a time.
                businessResponses = _client.GetGraphQlAsync(
                    _yelpIds.ToList(),
                    ct: default(CancellationToken),
                    connectionRetrySettings: null,
                    fragment: Client.DEFAULT_FRAGMENT).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }

            return View("Result", businessResponses);
        }

        // Test GetGraphQlInChunksAsync - 30 restaurants retrieved via multiple GraphQL calls (2 calls, 25 restaurants per call) made one at a time in about 7-8 seconds.
        public IActionResult TestGetGraphQlInChunksAsync()
        {
            IEnumerable<BusinessResponse> businessResponses;

            try
            {
                businessResponses = _client.GetGraphQlInChunksAsync(
                    _yelpIds.ToList(),
                    ct: default(CancellationToken),
                    connectionRetrySettings: null,
                    chunkSize: 25,
                    fragment: Client.DEFAULT_FRAGMENT).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }

            return View("Result", businessResponses);
        }

        // Test GetGraphQlInChunksAsyncInParallel - 30 restaurants retrieved via multiple GraphQL calls (2 calls, 25 restaurants per call) made in parallel in about 4-5 seconds.
        public IActionResult TestGetGraphQlInChunksAsyncInParallel()
        {
            IEnumerable<BusinessResponse> businessResponses;

            try
            {
                businessResponses = _client.GetGraphQlInChunksAsyncInParallel(
                    _yelpIds.ToList(),
                    ct: default(CancellationToken),
                    connectionRetrySettings: null,
                    chunkSize: 25,
                    fragment: Client.DEFAULT_FRAGMENT,
                    maxThreads: 2).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }

            return View("Result", businessResponses);
        }

        #endregion

        public IActionResult TestSearchBusinessesAllAsync()
        {
            var request = new SearchRequest
            {
                MaxResults = 50,

                Categories = "restaurants",
                Location = "Redwood City, CA 94065",
                OpenNow = false,
                Price = null,
                Radius = 8047,
                Term = null
            };

            var searchResponse = _client.SearchBusinessesAllAsync(request).Result;

            return View("Result", searchResponse.Businesses);
        }
    }
}