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
    zip_code
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
        private const int FIRST_TIME_WAIT = 500;

        private string AppID { get; set; }
        private string AppSecret { get; set; }
        private TokenResponse Token { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for the Client class.
        /// </summary>
        /// <param name="appID">App ID from yelp's developer registration page.</param>
        /// <param name="appSecret">App secret from yelp's developer registration page.</param>
        /// <param name="logger">Optional class instance which applies the ILogger interface to support custom logging within the client.</param>
        public Client(string appID, string appSecret, ILogger logger = null) 
            : base(BASE_ADDRESS, logger)
        {
            if (string.IsNullOrWhiteSpace(appID))
                throw new ArgumentNullException(nameof(appID));
            if (string.IsNullOrWhiteSpace(appSecret))
                throw new ArgumentNullException(nameof(appSecret));

            this.AppID = appID;
            this.AppSecret = appSecret;
        }

        #endregion

        #region Methods

        #region Authorization

        /// <summary>
        /// Retrieves a token value used for authentication of API calls.
        /// </summary>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <returns></returns>
        private async Task<TokenResponse> GetTokenAsync(CancellationToken ct = default(CancellationToken))
        {
            if (this.Token == null)
            {
                var dic = new Dictionary<string, string>();
                dic.Add("grant_type", "client_credentials");
                dic.Add("client_id", this.AppID);
                dic.Add("client_secret", this.AppSecret);
                var contents = new System.Net.Http.FormUrlEncodedContent(dic.ToKeyValuePairList());

                this.Token = await this.PostAsync<TokenResponse>("/oauth2/token", ct, contents);
            }

            return this.Token;
        }

        private async Task ApplyAuthenticationHeaders(CancellationToken ct = default(CancellationToken))
        {
            var token = await this.GetTokenAsync(ct);
            if (token?.Error != null)
                throw new Exception($"Could not retrieve authentication token: {token.Error?.Code} - {token.Error?.Description}");
            else  if (!string.IsNullOrEmpty(token?.AccessToken))
                this.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
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
        /// <returns>SearchResponse with businesses matching the specified parameters.</returns>
        public async Task<SearchResponse> SearchBusinessesWithDeliveryAsync(string term, double latitude, double longitude, CancellationToken ct = default(CancellationToken))
        {
            this.ValidateCoordinates(latitude, longitude);
            await this.ApplyAuthenticationHeaders(ct);

            var dic = new Dictionary<string, object>();
            if(!string.IsNullOrEmpty(term))
                dic.Add("term", term);
            dic.Add("latitude", latitude);
            dic.Add("longitude", longitude);
            string querystring = dic.ToQueryString();
            var response = await this.GetAsync<SearchResponse>(API_VERSION + "/transactions/delivery/search" + querystring, ct);

            // Set distances baased on lat/lon
            if (response?.Businesses != null && latitude != double.NaN && longitude != double.NaN)
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
        /// <returns>SearchResponse with businesses matching the specified parameters.</returns>
        public Task<SearchResponse> SearchBusinessesAllAsync(string term, double latitude, double longitude, CancellationToken ct = default(CancellationToken))
        {
            SearchRequest search = new SearchRequest();
            if (!string.IsNullOrEmpty(term))
                search.Term = term;
            search.Latitude = latitude;
            search.Longitude = longitude;
            return this.SearchBusinessesAllAsync(search, ct);
        }

        /// <summary>
        /// Searches any and all businesses matching the data in the specified search parameter object.
        /// </summary>
        /// <param name="search">Container object for all search parameters.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <returns>SearchResponse with businesses matching the specified parameters.</returns>
        public async Task<SearchResponse> SearchBusinessesAllAsync(SearchRequest search, CancellationToken ct = default(CancellationToken))
        {
            if (search == null)
                throw new ArgumentNullException(nameof(search));

            this.ValidateCoordinates(search.Latitude, search.Longitude);
            await this.ApplyAuthenticationHeaders(ct);

            var querystring = search.GetChangedProperties().ToQueryString();
            var response = await this.GetAsync<SearchResponse>(API_VERSION + "/businesses/search" + querystring, ct);

            // Set distances baased on lat/lon
            if (response?.Businesses != null && search.Latitude != double.NaN && search.Longitude != double.NaN)
                foreach (var business in response.Businesses)
                    business.SetDistanceAway(search.Latitude, search.Longitude);

            return response;
        }

        #endregion
        
        #region Autocomplete

        /// <summary>
        /// Searches businesses matching the specified search text used in a client search autocomplete box.
        /// </summary>
        /// <param name="term">Text to search businesses with.</param>
        /// <param name="latitude">User's current latitude.</param>
        /// <param name="longitude">User's current longitude.</param>
        /// <param name="locale">Language/locale value from https://www.yelp.com/developers/documentation/v3/supported_locales </param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <returns>AutocompleteResponse with businesses/categories/terms matching the specified parameters.</returns>
        public async Task<AutocompleteResponse> AutocompleteAsync(string text, double latitude, double longitude, string locale = null, CancellationToken ct = default(CancellationToken))
        {
            this.ValidateCoordinates(latitude, longitude);
            await this.ApplyAuthenticationHeaders(ct);

            var dic = new Dictionary<string, object>();
            dic.Add("text", text);
            dic.Add("latitude", latitude);
            dic.Add("longitude", longitude);
            if(!string.IsNullOrEmpty(locale))
                dic.Add("locale", locale);
            string querystring = dic.ToQueryString();

            var response = await this.GetAsync<AutocompleteResponse>(API_VERSION + "/autocomplete" + querystring, ct);

            // Set distances baased on lat/lon
            if (response?.Businesses != null && latitude != double.NaN && longitude != double.NaN)
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
        /// <returns>BusinessResponse instance with details of the specified business if found.</returns>
        public async Task<BusinessResponse> GetBusinessAsync(string businessID, CancellationToken ct = default(CancellationToken))
        {
            await this.ApplyAuthenticationHeaders(ct);            
            return await this.GetAsync<BusinessResponse>(API_VERSION + "/businesses/" + Uri.EscapeUriString(businessID), ct);
        }

        /// <summary>
        /// This method will retreive a list of Businesses from Yelp with separate calls.
        /// However, those calls will be made in parallel so while many calls will be made, the total
        /// results should be fast.
        /// Written in part with: https://stackoverflow.com/a/39796934/311444
        /// </summary>
        /// <param name="businessIds">A list of Yelp Business Ids to request from the GraphQL endpoint.</param>
        /// <param name="semaphoreSlimMax">The max amount of calls to be made at one time by SemaphoreSlim.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <returns>Returns an IEnumerable of BusinessResponses for each submitted businessId, wrapped in a Task.</returns>
        public async Task<IEnumerable<BusinessResponse>> GetBusinessAsyncInParallel(IEnumerable<string> businessIds, int semaphoreSlimMax = 10, CancellationToken ct = default(CancellationToken))
        {
            var tasks = new List<Task<BusinessResponse>>();

            SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, semaphoreSlimMax);

            bool firstTime = true;
            foreach (var id in businessIds)
            {
                await semaphoreSlim.WaitAsync(ct);
                tasks.Add(ProcessSemaphoreSlimsForGetBusinessAsync(semaphoreSlim, id, ct));

                // If first time, sleep so the oAuth token can be retreived before making all the other calls.
                if (firstTime)
                {
                    Task.Delay(FIRST_TIME_WAIT, ct).Wait(ct);
                    firstTime = false;
                }
            }

            return Task.WhenAll(tasks).Result;
        }

        /// <summary>
        /// This method processes the Semaphore wrapper around GetBusinessAsync calls.
        /// Written in part with: https://stackoverflow.com/a/39796934/311444
        /// </summary>
        /// <param name="semaphoreSlim">The Semaphore being used by the calling method.</param>
        /// <param name="businessId">The Yelp Business Id to request from the GetBusiness endpoint.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <returns>The BusinessResponses from the GetBusiness endpoint wrapped in a Task.</returns>
        private async Task<BusinessResponse> ProcessSemaphoreSlimsForGetBusinessAsync(SemaphoreSlim semaphoreSlim, string businessId, CancellationToken ct = default(CancellationToken))
        {
            Task<BusinessResponse> result;

            try
            {
                result = GetBusinessAsync(businessId, ct);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            await result;

            return result.Result;
        }

        #endregion

        #region Reviews

        /// <summary>
        /// Gets user reviews of a business based on the provided ID value.
        /// </summary>
        /// <param name="businessID">ID value of the Yelp business.</param>
        /// <param name="locale">Language/locale value from https://www.yelp.com/developers/documentation/v3/supported_locales </param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <returns>ReviewsResponse instance with reviews of the specified business if found.</returns>
        public async Task<ReviewsResponse> GetReviewsAsync(string businessID, string locale = null, CancellationToken ct = default(CancellationToken))
        {
            await this.ApplyAuthenticationHeaders(ct);

            var dic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(locale))
                dic.Add("locale", locale);
            string querystring = dic.ToQueryString();

            return await this.GetAsync<ReviewsResponse>(API_VERSION + $"/businesses/{Uri.EscapeUriString(businessID)}/reviews" + querystring, ct);
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
        /// <param name="fragment">The search fragment to be used on all requested Business Ids.  The DEFAULT_FRAGMENT is used by default.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <returns>A task of an IEnumerable of all the BusinessResponses from the GraphQL API.</returns>
        public async Task<IEnumerable<BusinessResponse>> GetGraphQlAsync(List<string> businessIds, string fragment = DEFAULT_FRAGMENT, CancellationToken ct = default(CancellationToken))
        {
            if (!businessIds.Any())
            {
                return new List<BusinessResponse>();
            }

            var content = new StringContent(CreateJsonRequestForGraphQl(businessIds, fragment), Encoding.UTF8, "application/x-www-form-urlencoded");

            await ApplyAuthenticationHeaders(ct);
            var jsonResponse = await PostAsync(API_VERSION + "/graphql", ct, content);

            return ConvertJsonToBusinesResponses(jsonResponse);
        }

        /// <summary>
        /// Private method that programmatically creates the JSON request for the GraphQL.
        /// </summary>
        /// <param name="businessIds">A list of Yelp Business Ids to request from the GraphQL endpoint.</param>
        /// <param name="fragment">The search fragment to be used on all requested Business Ids.  The DEFAULT_FRAGMENT is used by default.</param>
        /// <returns>A JSON string to be sent to the GraphQL endpoint.</returns>
        private string CreateJsonRequestForGraphQl(List<string> businessIds, string fragment = DEFAULT_FRAGMENT)
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
        /// This method will take the list of businessIds and divide them into chunks.  These chunks will be submitted
        /// to the GraphQL endpoint separately.  All results will be waited for, stitched back together, and returned.
        /// This will make more calls to the GraphQL endpoint than GetGraphQlAsync, but each call will be faster.  However, all of these calls
        /// will be made in series and the final results of all calls will be returned.
        /// *** NOTE ***
        /// The GraphQL endpoint is currently only available in the Yelp Fusion (3.0) Api Beta.  
        /// To use these endpoints, you have to go to Manage App and opt into the Beta.
        /// </summary>
        /// <param name="businessIds">A list of Yelp Business Ids to request from the GraphQL endpoint.</param>
        /// <param name="chunkSize">How many businesses to submit on each request.</param>
        /// <param name="fragment">The search fragment to be used on all requested Business Ids.  The DEFAULT_FRAGMENT is used by default.</param>
        /// <param name="semaphoreSlimMax">The max amount of calls to be made at one time by SemaphoreSlim.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <returns>A list of all BusinessResponses returned by every call to the GraphQL endpoint.</returns>
        public async Task<IEnumerable<BusinessResponse>> GetGraphQlInChunksAsync(
            List<string> businessIds,
            int chunkSize = 5,
            string fragment = DEFAULT_FRAGMENT,
            int semaphoreSlimMax = 10,
            CancellationToken ct = default(CancellationToken))
        {
            List<BusinessResponse> businessResponses = new List<BusinessResponse>();

            var graphResults = GetGraphQlAsyncInParallel(businessIds, chunkSize, fragment, semaphoreSlimMax, ct);

            var businessResponseLists = await Task.WhenAll(graphResults);

            foreach (var businessResponseList in businessResponseLists)
            {
                businessResponses.AddRange(businessResponseList);
            }

            return businessResponses;
        }

        /// <summary>
        /// This method runs in parallel, for a series version please see GetGraphQlInChunksAsync.
        /// This method will take the list of businessIds and divide them into chunks.  These chunks will be submitted
        /// to the GraphQL endpoint separately.  The Tasks of the requests will be put into a list for the user to await.
        /// This allows the call to be parallelizable.
        /// This will make more calls to the GraphQL endpoint than GetGraphQlAsync, but each call will be faster.  However, all of these calls
        /// will be made in series and the final results of all calls will be returned.
        /// The calls are done in parallel so it'll be faster than both GetGraphQlAsync and GetGraphQlInChunksAsync.
        /// Written in part with: https://stackoverflow.com/a/39796934/311444
        /// *** NOTE ***
        /// The GraphQL endpoint is currently only available in the Yelp Fusion (3.0) Api Beta.  
        /// To use these endpoints, you have to go to Manage App and opt into the Beta.
        /// </summary>
        /// <param name="businessIds">A list of Yelp Business Ids to request from the GraphQL endpoint.</param>
        /// <param name="chunkSize">How many businesses to submit on each request.</param>
        /// <param name="fragment">The search fragment to be used on all requested Business Ids.  The DEFAULT_FRAGMENT is used by default.</param>
        /// <param name="semaphoreSlimMax">The max amount of calls to be made at one time by SemaphoreSlim.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <returns>
        /// A list of Tasks where each Task contains an IEnumerable of BusinessResponses.  The caller will have to await for the Tasks 
        /// to return to get the results.
        /// </returns>
        public List<Task<IEnumerable<BusinessResponse>>> GetGraphQlAsyncInParallel(
            List<string> businessIds,
            int chunkSize = 5,
            string fragment = DEFAULT_FRAGMENT,
            int semaphoreSlimMax = 10,
            CancellationToken ct = default(CancellationToken))
        {
            int page = 0;
            int totalBusinessIds = businessIds.Count;
            int maxPage = (totalBusinessIds / chunkSize) + 1;

            var tasks = new List<Task<IEnumerable<BusinessResponse>>>();

            SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, semaphoreSlimMax);

            bool firstTime = true;
            do
            {
                semaphoreSlim.WaitAsync(ct);

                var idSubset = businessIds.Skip(page * chunkSize).Take(chunkSize).ToList();
                tasks.Add(ProcessSemaphoreSlimsForGraphQl(semaphoreSlim, idSubset, ct: ct));

                // If first time, sleep so the oAuth token can be retreived before making all the other calls.
                if (firstTime)
                {
                    Task.Delay(FIRST_TIME_WAIT, ct).Wait(ct);
                    firstTime = false;
                }

                page++;
            } while (page < maxPage);

            return tasks;
        }

        /// <summary>
        /// This method processes the Semaphore wrapper around GetGraphQlAsync calls.
        /// Written in part with: https://stackoverflow.com/a/39796934/311444
        /// </summary>
        /// <param name="semaphoreSlim">The Semaphore being used by the calling method.</param>
        /// <param name="businessIds">A list of Yelp Business Ids to request from the GraphQL endpoint.</param>
        /// <param name="fragment">The search fragment to be used on all requested Business Ids.  The DEFAULT_FRAGMENT is used by default.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <returns>A list of BusinessResponses from the GraphQL endpoint wrapped in a Task.</returns>
        private async Task<IEnumerable<BusinessResponse>> ProcessSemaphoreSlimsForGraphQl(
            SemaphoreSlim semaphoreSlim,
            List<string> businessIds,
            string fragment = DEFAULT_FRAGMENT,
            CancellationToken ct = default(CancellationToken))
        {
            Task<IEnumerable<BusinessResponse>> result;

            try
            {
                result = GetGraphQlAsync(businessIds, fragment, ct);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            await result;

            return result.Result;
        }

        /// <summary>
        /// This method is a utility method for processing the results of the GetGraphQlInParallel function.  It 
        /// takes all of the completed tasks, gets the Lists of BusinessResponses out of them, and puts them all into one list.
        /// Call this AFTER you have awaited GetGraphQlInParallel.
        /// *** NOTE ***
        /// The GraphQL endpoint is currently only available in the Yelp Fusion (3.0) Api Beta.  
        /// To use these endpoints, you have to go to Manage App and opt into the Beta.
        /// </summary>
        /// <param name="tasks">List of Tasks of IEnumerable BusinessResponses from GetGraphQlInParallel.</param>
        /// <returns>The complete list of all BusinessResponses from the GraphQL.</returns>
        public IEnumerable<BusinessResponse> ProcessResultsOfGetGraphQlAsyncInParallel(List<Task<IEnumerable<BusinessResponse>>> tasks)
        {
            List<BusinessResponse> businessResponses = new List<BusinessResponse>();

            foreach (var task in tasks)
            {
                businessResponses.AddRange(task.Result);
            }

            return businessResponses;
        }

        #endregion

        #endregion

        #endregion
    }
}
