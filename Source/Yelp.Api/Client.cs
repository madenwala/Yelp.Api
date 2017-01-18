using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Yelp.Api.Model;

namespace Yelp.Api
{
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

        public async Task<TokenResponse> GetTokenAsync(CancellationToken ct)
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
            if(!string.IsNullOrEmpty(token?.AccessToken))
                this.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }

        #endregion

        public async Task<SearchResponse> SearchBusinessesWithDeliveryAsync(string term, double latitude, double longitude, CancellationToken ct)
        {
            this.ValidateCoordinates(latitude, longitude);
            await this.ApplyAuthenticationHeaders(ct);

            var dic = new Dictionary<string, object>();
            if(!string.IsNullOrEmpty(term))
                dic.Add("term", term);
            dic.Add("latitude", latitude);
            dic.Add("longitude", longitude);
            string querystring = dic.ToQueryString();
            return await this.GetAsync<SearchResponse>(API_VERSION + "/transactions/delivery/search" + querystring, ct);
        }

        public async Task<SearchResponse> SearchBusinessesAllAsync(string term, double latitude, double longitude, CancellationToken ct)
        {
            this.ValidateCoordinates(latitude, longitude);
            await this.ApplyAuthenticationHeaders(ct);

            var dic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(term))
                dic.Add("term", term);
            dic.Add("latitude", latitude);
            dic.Add("longitude", longitude);
            string querystring = dic.ToQueryString();
            return await this.GetAsync<SearchResponse>(API_VERSION + "/businesses/search" + querystring, ct);
        }

        public async Task<AutocompleteResponse> AutocompleteAsync(string text, double latitude, double longitude, CancellationToken ct)
        {
            this.ValidateCoordinates(latitude, longitude);
            await this.ApplyAuthenticationHeaders(ct);

            var dic = new Dictionary<string, object>();
            dic.Add("text", text);
            dic.Add("latitude", latitude);
            dic.Add("longitude", longitude);
            string querystring = dic.ToQueryString();
            return await this.GetAsync<AutocompleteResponse>(API_VERSION + "/autocomplete" + querystring, ct);
        }

        public async Task<Business> GetBusinessAsync(string businessID, CancellationToken ct)
        {
            await this.ApplyAuthenticationHeaders(ct);            
            return await this.GetAsync<Business>(API_VERSION + "/businesses/" + Uri.EscapeUriString(businessID), ct);
        }

        public async Task<ReviewsResponse> GetReviewsAsync(string businessID, CancellationToken ct)
        {
            await this.ApplyAuthenticationHeaders(ct);
            return await this.GetAsync<ReviewsResponse>(API_VERSION + $"/businesses/{Uri.EscapeUriString(businessID)}/reviews", ct);
        }

        #region Validation

        private void ValidateCoordinates(double latitude, double longitude)
        {
        }

        #endregion

        #endregion
    }
}