using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
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

        #endregion
    }
}