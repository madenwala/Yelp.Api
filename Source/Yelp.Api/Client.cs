using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Yelp.Api.Models;

namespace Yelp.Api
{
    /// <summary>
    /// Client class to access Yelp Fusion v3 API.
    /// </summary>
    public sealed class Client : ClientBase
    {
        #region Variables

        private const string BASE_ADDRESS = "https://api.yelp.com";
        private const string API_VERSION = "/v3";
        private const int FIRST_TIME_WAIT = 500;

        /// <summary>
        /// The default fragment used by GraphQL calls.  This is not a variable that can be set, it is only
        /// exposed so that the user can see what the default search behavior of the GraphQL will be.
        /// If a different search behavior is desired, please use the fragment variable in any GraphQL 
        /// function call.
        /// </summary>
        public const string DEFAULT_FRAGMENT = @"
id
alias
photos
name
url
rating
review_count
price
categories {
    title
    alias
}
location {
    address1
    address2
    address3
    city
    state
    postal_code
}
display_phone
coordinates {
    latitude
    longitude
}
hours {
    is_open_now
}
";
        private const string DEFAULT_FRAGMENT_NAME = "businessInfo";

        private string ApiKey { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for the Client class.
        /// </summary>
        /// <param name="apiKey">App secret from yelp's developer registration page.</param>
        /// <param name="logger">Optional class instance which applies the ILogger interface to support custom logging within the client.</param>
        public Client(string apiKey, ILogger logger = null) 
            : base(BASE_ADDRESS, logger)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            this.ApiKey = apiKey;
        }

        #endregion

        #region Methods

        #region Authorization

        private void ApplyAuthenticationHeaders(CancellationToken ct = default(CancellationToken))
        {
            this.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.ApiKey);
        }

        #endregion

        #region Search

        /// <summary>
        /// Searches businesses that deliver matching the specified search text.
        /// </summary>
        /// <param name="term">Text to search businesses with.</param>
        /// <param name="latitude">User's current latitude.</param>
        /// <param name="longitude">User's current longitude.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <param name="connectionRetrySettings">The settings to define whether a connection should be retried.</param>
        /// <returns>SearchResponse with businesses matching the specified parameters.</returns>
        public async Task<SearchResponse> SearchBusinessesWithDeliveryAsync(
            string term, 
            double latitude, 
            double longitude, 
            CancellationToken ct = default(CancellationToken),
            ConnectionRetrySettings connectionRetrySettings = null)
        {
            this.ValidateCoordinates(latitude, longitude);
            this.ApplyAuthenticationHeaders(ct);

            var dic = new Dictionary<string, object>();
            if(!string.IsNullOrEmpty(term))
                dic.Add("term", term);
            dic.Add("latitude", latitude);
            dic.Add("longitude", longitude);
            string querystring = dic.ToQueryString();
            var response = await this.GetAsync<SearchResponse>(API_VERSION + "/transactions/delivery/search" + querystring, ct, connectionRetrySettings);

            // Set distances based on lat/lon
            if (response?.Businesses != null && !double.IsNaN(latitude) && !double.IsNaN(longitude))
                foreach (var business in response.Businesses)
                    business.SetDistanceAway(latitude, longitude);

            return response;
        }

        /// <summary>
        /// Searches any and all businesses matching the specified search text.
        /// </summary>
        /// <param name="term">Text to search businesses with.</param>
        /// <param name="latitude">User's current latitude.</param>
        /// <param name="longitude">User's current longitude.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <param name="connectionRetrySettings">The settings to define whether a connection should be retried.</param>
        /// <returns>SearchResponse with businesses matching the specified parameters.</returns>
        public Task<SearchResponse> SearchBusinessesAllAsync(
            string term, 
            double latitude, 
            double longitude, 
            CancellationToken ct = default(CancellationToken),
            ConnectionRetrySettings connectionRetrySettings = null)
        {
            SearchRequest search = new SearchRequest();
            if (!string.IsNullOrEmpty(term))
                search.Term = term;
            search.Latitude = latitude;
            search.Longitude = longitude;
            return this.SearchBusinessesAllAsync(search, ct, connectionRetrySettings);
        }

        /// <summary>
        /// Searches any and all businesses matching the data in the specified search parameter object.
        /// </summary>
        /// <param name="search">Container object for all search parameters.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <param name="connectionRetrySettings">The settings to define whether a connection should be retried.</param>
        /// <returns>SearchResponse with businesses matching the specified parameters.</returns>
        public async Task<SearchResponse> SearchBusinessesAllAsync(
            SearchRequest search, 
            CancellationToken ct = default(CancellationToken),
            ConnectionRetrySettings connectionRetrySettings = null)
        {
            if (search == null)
                throw new ArgumentNullException(nameof(search));

            this.ValidateCoordinates(search.Latitude, search.Longitude);
            this.ApplyAuthenticationHeaders(ct);

            var querystring = search.GetChangedProperties().ToQueryString();
            var response = await this.GetAsync<SearchResponse>(API_VERSION + "/businesses/search" + querystring, ct, connectionRetrySettings);

            // Set distances baased on lat/lon
            if (response?.Businesses != null && !double.IsNaN(search.Latitude) && !double.IsNaN(search.Longitude))
                foreach (var business in response.Businesses)
                    business.SetDistanceAway(search.Latitude, search.Longitude);

            return response;
        }

        #endregion

        #region Autocomplete

        /// <summary>
        /// Searches businesses matching the specified search text used in a client search autocomplete box.
        /// </summary>
        /// <param name="text">Text to search businesses with.</param>
        /// <param name="latitude">User's current latitude.</param>
        /// <param name="longitude">User's current longitude.</param>
        /// <param name="locale">Language/locale value from https://www.yelp.com/developers/documentation/v3/supported_locales </param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <param name="connectionRetrySettings">The settings to define whether a connection should be retried.</param>
        /// <returns>AutocompleteResponse with businesses/categories/terms matching the specified parameters.</returns>
        public async Task<AutocompleteResponse> AutocompleteAsync(
            string text, 
            double latitude, 
            double longitude, 
            string locale = null, 
            CancellationToken ct = default(CancellationToken),
            ConnectionRetrySettings connectionRetrySettings = null)
        {
            this.ValidateCoordinates(latitude, longitude);
            this.ApplyAuthenticationHeaders(ct);

            var dic = new Dictionary<string, object>();
            dic.Add("text", text);
            dic.Add("latitude", latitude);
            dic.Add("longitude", longitude);
            if(!string.IsNullOrEmpty(locale))
                dic.Add("locale", locale);
            string querystring = dic.ToQueryString();

            var response = await this.GetAsync<AutocompleteResponse>(API_VERSION + "/autocomplete" + querystring, ct, connectionRetrySettings);

            // Set distances baased on lat/lon
            if (response?.Businesses != null && !double.IsNaN(latitude) && !double.IsNaN(longitude))
                foreach (var business in response.Businesses)
                    business.SetDistanceAway(latitude, longitude);

            return response;
        }

        #endregion

        #region Business Details

        /// <summary>
        /// Gets details of a business based on the provided ID value.
        /// </summary>
        /// <param name="businessID">ID value of the Yelp business.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <param name="connectionRetrySettings">The settings to define whether a connection should be retried.</param>
        /// <returns>BusinessResponse instance with details of the specified business if found.</returns>
        public async Task<BusinessResponse> GetBusinessAsync(
            string businessID, 
            CancellationToken ct = default(CancellationToken),
            ConnectionRetrySettings connectionRetrySettings = null)
        {
            this.ApplyAuthenticationHeaders(ct);            
            return await this.GetAsync<BusinessResponse>(API_VERSION + "/businesses/" + Uri.EscapeUriString(businessID), ct, connectionRetrySettings);
        }

        /// <summary>
        /// This method will retrieve a list of Businesses from Yelp with separate calls.
        /// However, those calls will be made in parallel so while many calls will be made, the total
        /// results should be fast.
        /// Written in part with: https://stackoverflow.com/a/39796934/311444 and https://stackoverflow.com/a/23316722/311444
        /// </summary>
        /// <param name="businessIds">A list of Yelp Business Ids to request from the GraphQL endpoint.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <param name="maxThreads">
        ///     The max amount of calls to be made at one time by SemaphoreSlim. 2 is the recommended amount.
        ///     More threads would mean more calls at once, but a greater chance of getting calls rejected by Yelp.
        /// </param>
        /// <param name="connectionRetrySettings">The settings to define whether a connection should be retried.</param>
        /// <returns>Returns an IEnumerable of BusinessResponses for each submitted businessId, wrapped in a Task.</returns>
        public async Task<IEnumerable<BusinessResponse>> GetBusinessAsyncInParallel(
            IEnumerable<string> businessIds, 
            CancellationToken ct = default(CancellationToken),
            int maxThreads = 2,
            ConnectionRetrySettings connectionRetrySettings = null)
        {
            if (connectionRetrySettings == null)
            {
                connectionRetrySettings = new ConnectionRetrySettings();
            }

            SemaphoreSlim semaphoreSlim = new SemaphoreSlim(maxThreads, maxThreads);

            var businessResponses = new List<BusinessResponse>();
            await Task.WhenAll(businessIds.Select(async businessId =>
            {
                await semaphoreSlim.WaitAsync(ct);
                try
                {
                    // You have to create a separate object for each Semaphore, otherwise they all use the same counter and quickly run out of retries.
                    ConnectionRetrySettings connectionRetrySettingsForThisAttempt = new ConnectionRetrySettings(
                        connectionRetrySettings.CurrentTry, 
                        connectionRetrySettings.IsRetryConnections, 
                        connectionRetrySettings.MaxAmountOfTries);

                    businessResponses.Add(await GetBusinessAsync(businessId, ct, connectionRetrySettingsForThisAttempt));
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }));

            return businessResponses;
        }

        #endregion

        #region Reviews

        /// <summary>
        /// Gets user reviews of a business based on the provided ID value.
        /// </summary>
        /// <param name="businessID">ID value of the Yelp business.</param>
        /// <param name="locale">Language/locale value from https://www.yelp.com/developers/documentation/v3/supported_locales </param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <param name="connectionRetrySettings">The settings to define whether a connection should be retried.</param>
        /// <returns>ReviewsResponse instance with reviews of the specified business if found.</returns>
        public async Task<ReviewsResponse> GetReviewsAsync(
            string businessID, 
            string locale = null, 
            CancellationToken ct = default(CancellationToken),
            ConnectionRetrySettings connectionRetrySettings = null)
        {
            this.ApplyAuthenticationHeaders(ct);

            var dic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(locale))
                dic.Add("locale", locale);
            string querystring = dic.ToQueryString();

            return await this.GetAsync<ReviewsResponse>(API_VERSION + $"/businesses/{Uri.EscapeUriString(businessID)}/reviews" + querystring, ct, connectionRetrySettings);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates latitude and longitude values. Throws an ArgumentOutOfRangeException if not in the valid range of values.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        private void ValidateCoordinates(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentOutOfRangeException(nameof(latitude));
            else if (longitude < -180 || latitude > 180)
                throw new ArgumentOutOfRangeException(nameof(longitude));
        }

        #endregion

        #region GraphQL

        /*
         * The GraphQL endpoint is currently only available in the Yelp Fusion (3.0) Api Beta.  
         * To use these endpoints, you have to go to Manage App and opt into the Beta.
         */

        #region Individual Graph Request

        /// <summary>
        /// This method makes a single request to the Yelp GraphQL endpoint.  
        /// It formats the entire list of businessIds and the search fragment into the proper json to make the request.
        /// *** NOTE ***
        /// The GraphQL endpoint is currently only available in the Yelp Fusion (3.0) Api Beta.  
        /// To use these endpoints, you have to go to Manage App and opt into the Beta.
        /// </summary>
        /// <param name="businessIds">A list of Yelp Business Ids to request from the GraphQL endpoint.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <param name="connectionRetrySettings">The settings to define whether a connection should be retried.</param>
        /// <param name="fragment">The search fragment to be used on all requested Business Ids.  The DEFAULT_FRAGMENT is used by default.</param>
        /// <returns>A task of an IEnumerable of all the BusinessResponses from the GraphQL API.</returns>
        public async Task<IEnumerable<BusinessResponse>> GetGraphQlAsync(
            List<string> businessIds,
            CancellationToken ct = default(CancellationToken),
            ConnectionRetrySettings connectionRetrySettings = null,
            string fragment = DEFAULT_FRAGMENT)
        {
            if (!businessIds.Any())
            {
                return new List<BusinessResponse>();
            }

            var httpConnectionSettings = new HttpConnectionSettings
            {
                Content = CreateRequestBodyForGraphQl(businessIds, fragment),
                Encoding = Encoding.UTF8,
                MediaType = "application/graphql"
            };
            
            ApplyAuthenticationHeaders(ct);
            var jsonResponse = await PostAsync(API_VERSION + "/graphql", ct, httpConnectionSettings, connectionRetrySettings);

            return ConvertJsonToBusinesResponses(jsonResponse);
        }

        /// <summary>
        /// Private method that programmatically creates the request body for the GraphQL.
        /// </summary>
        /// <param name="businessIds">A list of Yelp Business Ids to request from the GraphQL endpoint.</param>
        /// <param name="fragment">The search fragment to be used on all requested Business Ids.  The DEFAULT_FRAGMENT is used by default.</param>
        /// <returns>A JSON string to be sent to the GraphQL endpoint.</returns>
        private string CreateRequestBodyForGraphQl(List<string> businessIds, string fragment = DEFAULT_FRAGMENT)
        {
            string body = "{ ";
            int x = 1;

            foreach (var businessId in businessIds)
            {
                body += $@"
b{x++}: business(id: ""{businessId}"") {{ 
    ...{DEFAULT_FRAGMENT_NAME} 
}} ";
            }
            body += "\n} \n";
            body += $@"
fragment {DEFAULT_FRAGMENT_NAME} on Business {{ 
    {fragment}
}} 
";

            return body;
        }

        /// <summary>
        /// A private method that takes the response from the GraphQL endpoint and converts it into a list of BusinessResponse objects.
        /// </summary>
        /// <param name="jsonResponse">The JSON response from the GraphQL endpoint.</param>
        /// <returns>A list of BusinessResponses parsed from the JSON response string.</returns>
        private IEnumerable<BusinessResponse> ConvertJsonToBusinesResponses(string jsonResponse)
        {
            var jObject = JObject.Parse(jsonResponse);
            var businessResponseDictionary = jObject["data"].ToObject<Dictionary<string, BusinessResponse>>();

            return businessResponseDictionary.Select(keyValuePair => keyValuePair.Value).ToList();
        }

        #endregion

        #region Parallelizable Graph Requests

        /// <summary>
        /// This method runs in series, for a parallel version please see GetGraphQlAsyncInParallel.
        /// This method will take the list of businessIds and divide them into chunks.
        /// These chunks will be submitted to the GraphQL endpoint separately one after the other.  
        /// This will make more calls to the GraphQL endpoint than GetGraphQlAsync, but each call will only be a portion of the
        /// total businesses so it should run faster than GetGraphQlAsync.  
        /// *** NOTE ***
        /// The GraphQL endpoint is currently only available in the Yelp Fusion (3.0) Api Beta.  
        /// To use these endpoints, you have to go to Manage App and opt into the Beta.
        /// </summary>
        /// <param name="businessIds">A list of Yelp Business Ids to request from the GraphQL endpoint.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <param name="connectionRetrySettings">The settings to define whether a connection should be retried.</param>
        /// <param name="chunkSize">
        ///     How many businesses to submit on each request.  25 is the recommended amount.
        ///     Submitting more at one time will make the call to Yelp take longer, but there will be less calls to Yelp overall.
        ///     Submitting less at one time will make the calls to Yelp quicker, but there will be more calls to Yelp overall.
        /// </param>
        /// <param name="fragment">The search fragment to be used on all requested Business Ids.  The DEFAULT_FRAGMENT is used by default.</param>
        /// <param name="maxThreads">
        ///     The max amount of calls to be made at one time by SemaphoreSlim. 2 is the recommended amount.
        ///     More threads would mean more calls at once, but a greater chance of getting calls rejected by Yelp.
        /// </param>
        /// <returns>A list of all BusinessResponses returned by every call to the GraphQL endpoint.</returns>
        public async Task<IEnumerable<BusinessResponse>> GetGraphQlInChunksAsync(
            List<string> businessIds,
            CancellationToken ct = default(CancellationToken),
            ConnectionRetrySettings connectionRetrySettings = null,
            int chunkSize = 25,
            string fragment = DEFAULT_FRAGMENT)
        {
            return await GetGraphQlInChunksAsyncInParallel(businessIds, ct, connectionRetrySettings, chunkSize, fragment, 1);
        }

        /// <summary>
        /// This method runs in parallel, for a version that runs in series please see GetGraphQlInChunksAsync.
        /// This method will take the list of businessIds and divide them into chunks.
        /// They will be submitted in parallel as defined by the size of the SemaphoreSlim.
        /// This will make more calls to the GraphQL endpoint than GetGraphQlAsync, but each call will only be a portion of the
        /// total businesses so it should run faster than GetGraphQlAsync.  
        /// The calls are done in parallel so it'll be faster than both GetGraphQlAsync and GetGraphQlInChunksAsync.
        /// Written in part with: https://stackoverflow.com/a/39796934/311444 and https://stackoverflow.com/a/23316722/311444
        /// *** NOTE ***
        /// The GraphQL endpoint is currently only available in the Yelp Fusion (3.0) Api Beta.  
        /// To use these endpoints, you have to go to Manage App and opt into the Beta.
        /// </summary>
        /// <param name="businessIds">A list of Yelp Business Ids to request from the GraphQL endpoint.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <param name="connectionRetrySettings">The settings to define whether a connection should be retried.</param>
        /// <param name="chunkSize">
        ///     How many businesses to submit on each request.  25 is the recommended amount.
        ///     Submitting more at one time will make the call to Yelp take longer, but there will be less calls to Yelp overall.
        ///     Submitting less at one time will make the calls to Yelp quicker, but there will be more calls to Yelp overall.
        /// </param>
        /// <param name="fragment">The search fragment to be used on all requested Business Ids.  The DEFAULT_FRAGMENT is used by default.</param>
        /// <param name="maxThreads">
        ///     The max amount of calls to be made at one time by SemaphoreSlim. 2 is the recommended amount.
        ///     More threads would mean more calls at once, but a greater chance of getting calls rejected by Yelp.
        /// </param>
        /// <returns>
        /// A list of Tasks where each Task contains an IEnumerable of BusinessResponses.  The caller will have to await for the Tasks 
        /// to return to get the results.
        /// </returns>
        public async Task<IEnumerable<BusinessResponse>> GetGraphQlInChunksAsyncInParallel(
            List<string> businessIds,
            CancellationToken ct = default(CancellationToken),
            ConnectionRetrySettings connectionRetrySettings = null,
            int chunkSize = 25,
            string fragment = DEFAULT_FRAGMENT,
            int maxThreads = 2)
        {
            if (connectionRetrySettings == null)
            {
                connectionRetrySettings = new ConnectionRetrySettings();
            }

            SemaphoreSlim semaphoreSlim = new SemaphoreSlim(maxThreads, maxThreads);

            var businessResponses = new List<BusinessResponse>();
            var businessSubsets = GetSubsetsOfBusinessIds(businessIds, chunkSize);
            await Task.WhenAll(businessSubsets.Select(async subset =>
            {
                await semaphoreSlim.WaitAsync(ct);
                try
                {
                    // You have to create a separate object for each Semaphore, otherwise they all use the same counter and quickly run out of retries.
                    ConnectionRetrySettings connectionRetrySettingsForThisAttempt = new ConnectionRetrySettings(
                        connectionRetrySettings.CurrentTry,
                        connectionRetrySettings.IsRetryConnections,
                        connectionRetrySettings.MaxAmountOfTries);

                    businessResponses.AddRange(await GetGraphQlAsync(businessIds, ct, connectionRetrySettingsForThisAttempt, fragment));
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }));

            return businessResponses;
        }

        /// <summary>
        /// Method to convert a large number of strings into subsets based on the chunkSize.
        /// </summary>
        /// <param name="businessIds">Total list of all Business Ids.</param>
        /// <param name="chunkSize">The size of each subset.</param>
        /// <returns>A list of all subsets of Business Ids.</returns>
        private List<List<string>> GetSubsetsOfBusinessIds(List<string> businessIds, int chunkSize)
        {
            int page = 0;
            int totalBusinessIds = businessIds.Count;
            int maxPage = (totalBusinessIds / chunkSize) + 1;

            var businessSubsets = new List<List<string>>();
            do
            {
                var idSubset = businessIds.Skip(page * chunkSize).Take(chunkSize).ToList();
                if (idSubset.Any())
                {
                    businessSubsets.Add(idSubset);
                }

                page++;
            } while (page < maxPage);

            return businessSubsets;
        }

        #endregion

        #endregion

        #endregion
    }
}
