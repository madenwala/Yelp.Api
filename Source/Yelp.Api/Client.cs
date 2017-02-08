using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Yelp.Api.Models;

namespace Yelp.Api
{
    /// <summary>
    /// Client class to access Yelp API.
    /// </summary>
    public class Client : ClientBase
    {
        #region Variables

        private const string BASE_ADDRESS = "https://api.yelp.com";
        private const string API_VERSION = "/v3";

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
            : base("https://api.yelp.com", logger)
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

        private async Task<TokenResponse> GetTokenAsync(CancellationToken ct)
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

        private async Task ApplyAuthenticationHeaders(CancellationToken ct)
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
        public async Task<SearchResponse> SearchBusinessesWithDeliveryAsync(string term, double latitude, double longitude, CancellationToken? ct = null)
        {
            if (ct == null)
                ct = CancellationToken.None;

            this.ValidateCoordinates(latitude, longitude);
            await this.ApplyAuthenticationHeaders(ct.Value);

            var dic = new Dictionary<string, object>();
            if(!string.IsNullOrEmpty(term))
                dic.Add("term", Uri.EscapeUriString(term));
            dic.Add("latitude", latitude);
            dic.Add("longitude", longitude);
            string querystring = dic.ToQueryString();
            return await this.GetAsync<SearchResponse>(API_VERSION + "/transactions/delivery/search" + querystring, ct.Value);
        }

        /// <summary>
        /// Searches any and all businesses matching the specified search text.
        /// </summary>
        /// <param name="term">Text to search businesses with.</param>
        /// <param name="latitude">User's current latitude.</param>
        /// <param name="longitude">User's current longitude.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <returns>SearchResponse with businesses matching the specified parameters.</returns>
        public Task<SearchResponse> SearchBusinessesAllAsync(string term, double latitude, double longitude, CancellationToken? ct = null)
        {
            SearchParameters search = new SearchParameters();
            if (!string.IsNullOrEmpty(term))
                search.Term = term;
            search.Latitude = latitude;
            search.Longitude = longitude;
            return this.SearchBusinessesAllAsync(search, ct);
        }

        public async Task<SearchResponse> SearchBusinessesAllAsync(SearchParameters search, CancellationToken? ct = null)
        {
            if (search == null)
                throw new ArgumentNullException(nameof(search));
            if (ct == null)
                ct = CancellationToken.None;

            this.ValidateCoordinates(search.Latitude, search.Longitude);
            await this.ApplyAuthenticationHeaders(ct.Value);

            var querystring = search.GetChangedProperties().ToQueryString();
            return await this.GetAsync<SearchResponse>(API_VERSION + "/businesses/search" + querystring, ct.Value);
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
        public async Task<AutocompleteResponse> AutocompleteAsync(string text, double latitude, double longitude, string locale = null, CancellationToken? ct = null)
        {
            if (ct == null)
                ct = CancellationToken.None;

            this.ValidateCoordinates(latitude, longitude);
            await this.ApplyAuthenticationHeaders(ct.Value);

            var dic = new Dictionary<string, object>();
            dic.Add("text", Uri.EscapeUriString(text));
            dic.Add("latitude", latitude);
            dic.Add("longitude", longitude);
            if(!string.IsNullOrEmpty(locale))
                dic.Add("locale", Uri.EscapeUriString(locale));
            string querystring = dic.ToQueryString();

            return await this.GetAsync<AutocompleteResponse>(API_VERSION + "/autocomplete" + querystring, ct.Value);
        }

        #endregion

        #region Business Details

        /// <summary>
        /// Gets details of a business based on the provided ID value.
        /// </summary>
        /// <param name="businessID">ID value of the Yelp business.</param>
        /// <param name="ct">Cancellation token instance. Use CancellationToken.None if not needed.</param>
        /// <returns>BusinessResponse instance with details of the specified business if found.</returns>
        public async Task<BusinessResponse> GetBusinessAsync(string businessID, CancellationToken? ct)
        {
            if (ct == null)
                ct = CancellationToken.None;

            await this.ApplyAuthenticationHeaders(ct.Value);            
            return await this.GetAsync<BusinessResponse>(API_VERSION + "/businesses/" + Uri.EscapeUriString(businessID), ct.Value);
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
        public async Task<ReviewsResponse> GetReviewsAsync(string businessID, string locale = null, CancellationToken? ct = null)
        {
            if (ct == null)
                ct = CancellationToken.None;

            await this.ApplyAuthenticationHeaders(ct.Value);

            var dic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(locale))
                dic.Add("locale", Uri.EscapeUriString(locale));
            string querystring = dic.ToQueryString();

            return await this.GetAsync<ReviewsResponse>(API_VERSION + $"/businesses/{Uri.EscapeUriString(businessID)}/reviews" + querystring, ct.Value);
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