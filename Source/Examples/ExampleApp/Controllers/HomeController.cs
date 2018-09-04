using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        // 30 restaurants retrieved one a time in about 30-33 seconds.
        public IActionResult TestGetBusinessAsync()
        {
            List<BusinessResponse> businessResponses = new List<BusinessResponse>();

            foreach (var yelpId in _yelpIds)
            {
                BusinessResponse businessResponse = _client.GetBusinessAsync(yelpId).Result;
                businessResponses.Add(businessResponse);
            }

            return View("Result", businessResponses);
        }

        // 30 restaurants retrieved in parallel in about 2-3 seconds.
        public IActionResult TestGetBusinessAsyncInParallel()
        {
            IEnumerable<BusinessResponse> businessResponses = _client.GetBusinessAsyncInParallel(_yelpIds.ToList()).Result;

            return View("Result", businessResponses);
        }

        // 10 restaurants retrieved via one GraphQL call in about 3-4 seconds.
        public IActionResult TestGetGraphQlAsync()
        {
            // More than 10 items at a time seems to crash Yelp's API.  Nothing I can do about that.
            IEnumerable<BusinessResponse> businessResponses = _client.GetGraphQlAsync(_yelpIds.Take(10).ToList()).Result;

            return View("Result", businessResponses);
        }

        // 30 restaurants retrieved via multiple GraphQL calls made one at a time in about 3-4 seconds.
        public IActionResult TestGetGraphQlInChunksAsync()
        {
            IEnumerable<BusinessResponse> businessResponses = _client.GetGraphQlInChunksAsync(_yelpIds.ToList(), chunkSize: 5, semaphoreSlimMax: 10).Result;

            return View("Result", businessResponses);
        }

        // 30 restaurants retrieved via multiple GraphQL calls made in parallel in about 2-3 seconds.
        public IActionResult TestGetGraphQlAsyncInParallel()
        {
            IEnumerable<Task<IEnumerable<BusinessResponse>>> businessResponseTasks = _client.GetGraphQlAsyncInParallel(_yelpIds.ToList(), chunkSize: 5, semaphoreSlimMax: 10);
            IEnumerable<BusinessResponse> businessResponses = _client.ProcessResultsOfGetGraphQlAsyncInParallel(businessResponseTasks.ToList());

            return View("Result", businessResponses);
        }

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
