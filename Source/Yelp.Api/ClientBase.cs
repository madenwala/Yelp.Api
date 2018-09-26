using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Yelp.Api.Exceptions;
using Yelp.Api.Models;

namespace Yelp.Api
{
    /// <summary>
    /// Base class for any SDK client API implementation containing reusable logic for common call types, error handling, request retry attempts.
    /// </summary>
    public abstract class ClientBase : IDisposable
    {
        #region Variables

        protected HttpClient Client { get; private set; }

        protected Uri BaseUri { get; private set; }

        public const int E_WINHTTP_TIMEOUT = unchecked((int)0x80072ee2);
        public const int E_WINHTTP_NAME_NOT_RESOLVED = unchecked((int)0x80072ee7);
        public const int E_WINHTTP_CANNOT_CONNECT = unchecked((int)0x80072efd);
        public const int E_WINHTTP_CONNECTION_ERROR = unchecked((int)0x80072efe);

        private ILogger _logger;

        #endregion

        #region Constructors

        public ClientBase(string baseURL = null, ILogger logger = null)
        {
            this.BaseUri = new Uri(baseURL);
            this.Client = new HttpClient();
            _logger = logger;
        }

        public void Dispose()
        {
            this.Client.Dispose();
            this.Client = null;
        }

        #endregion

        #region Methods

        #region Get

        /// <summary>
        /// Gets data from the specified URL.
        /// </summary>
        /// <typeparam name="T">Type for the strongly typed class representing data returned from the URL.</typeparam>
        /// <param name="url">URL to retrieve data from.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <param name="connectionRetrySettings">The settings to define whether a connection should be retried.</param>
        /// <returns>Instance of the type specified representing the data returned from the URL.</returns>
        protected async Task<T> GetAsync<T>(string url, CancellationToken ct, ConnectionRetrySettings connectionRetrySettings)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            if (connectionRetrySettings == null)
            {
                connectionRetrySettings = new ConnectionRetrySettings();
            }

            var response = await this.Client.GetAsync(new Uri(this.BaseUri, url), ct);
            this.Log(response);
            var data = await response.Content.ReadAsStringAsync();

            if (DoesThisNeedToRetry(connectionRetrySettings, data, response.StatusCode))
            {
                connectionRetrySettings.CurrentTry++;
                return await GetAsync<T>(url, ct, connectionRetrySettings);
            }
            ThrowIfAccessLimitReached(data, response.StatusCode);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var jsonModel = JsonConvert.DeserializeObject<T>(data, settings);

            return jsonModel;
        }
        
        /// <summary>
        /// Posts data to the specified URL.
        /// </summary>
        /// <param name="url">URL to retrieve data from.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <param name="httpConnectionSettings">Settings to create the HttpContent value.  Doing it inside the method allows for connection retries.</param>
        /// <param name="connectionRetrySettings">The settings to define whether a connection should be retried.</param>
        /// <returns>Response contents as string else null if nothing.</returns>
        protected async Task<string> PostAsync(
            string url, 
            CancellationToken ct, 
            HttpConnectionSettings httpConnectionSettings,
            ConnectionRetrySettings connectionRetrySettings = null)
        {
            if (connectionRetrySettings == null)
            {
                connectionRetrySettings = new ConnectionRetrySettings();
            }

            HttpResponseMessage response = await this.PostAsync(
                url,
                new StringContent(httpConnectionSettings.Content, httpConnectionSettings.Encoding, httpConnectionSettings.MediaType), 
                ct);

            var data = await response.Content?.ReadAsStringAsync();
            
            if (DoesThisNeedToRetry(connectionRetrySettings, data, response.StatusCode))
            {
                connectionRetrySettings.CurrentTry++;
                return await PostAsync(url, ct, httpConnectionSettings, connectionRetrySettings);
            }
            ThrowIfAccessLimitReached(data, response.StatusCode);

            return data;
        }

        /// <summary>
        /// Posts data to the specified URL.
        /// </summary>
        /// <param name="url">URL to retrieve data from.</param>
        /// <param name="contents">Any content that should be passed into the post.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response contents as string else null if nothing.</returns>
        private async Task<HttpResponseMessage> PostAsync(string url, HttpContent contents, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var response = await this.Client.PostAsync(new Uri(this.BaseUri, url), contents, ct);
            this.Log(response);
            return response;
        }

        private bool DoesThisNeedToRetry(ConnectionRetrySettings connectionRetrySettings, string content, HttpStatusCode responseCode)
        {
            // TODO: 429 Too Many Requests was not included in .NET Core 1.0.  Change when upgrading to 2.0
            // TODO: Look into using this instead in 2.1 https://stackoverflow.com/a/35183487/311444
            if (Convert.ToInt32(responseCode) == 429)
            {
                if (content.Contains("You have exceeded the queries-per-second limit for this endpoint"))
                {
                    if (connectionRetrySettings.IsRetryConnections &&
                        connectionRetrySettings.CurrentTry <= connectionRetrySettings.MaxAmountOfTries)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ThrowIfAccessLimitReached(string content, HttpStatusCode responseCode)
        {
            // TODO: 429 Too Many Requests was not included in .NET Core 1.0.  Change when upgrading to 2.0
            // TODO: Look into using this instead in 2.1 https://stackoverflow.com/a/35183487/311444
            if (Convert.ToInt32(responseCode) == 429)
            {
                if (content.Contains("You've reached the access limit for this client"))
                {
                    throw new AccessLimitException(content);
                }
            }
        }

        #endregion

        #region Logging

        private void Log(string message)
        {
            if (_logger != null)
                _logger.Log(message);
            else
                System.Diagnostics.Debug.WriteLine(message);
        }

        /// <summary>
        /// Logs HttpRequest information to the application logger.
        /// </summary>
        /// <param name="request">Request to log.</param>
        private void Log(HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                var message = string.Format(
                    Environment.NewLine + "---------------------------------" + Environment.NewLine +
                    "WEB REQUEST to {0}" + Environment.NewLine +
                    "-Method: {1}" + Environment.NewLine +
                    "-Headers: {2}" + Environment.NewLine +
                    "-Contents: " + Environment.NewLine + "{3}" + Environment.NewLine +
                    "---------------------------------",
                    request.RequestUri.OriginalString,
                    request.Method.Method,
                    request.Headers,
                    request.Content
                );
                this.Log(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error during Log(HttpRequestMessage request): " + ex.ToString());
            }
        }

        /// <summary>
        /// Logs the HttpResponse object to the application logger.
        /// </summary>
        /// <param name="response">Response to log.</param>
        private void Log(HttpResponseMessage response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            this.Log(response.RequestMessage);

            try
            {
                var message = string.Format(
                    Environment.NewLine + "---------------------------------" + Environment.NewLine +
                    "WEB RESPONSE to {0}" + Environment.NewLine +
                    "-HttpStatus: {1}" + Environment.NewLine +
                    "-Reason Phrase: {2}" + Environment.NewLine +
                    "-ContentLength: {3:0.00 KB}" + Environment.NewLine +
                    "-Contents: " + Environment.NewLine + "{4}" + Environment.NewLine +
                    "---------------------------------",
                    response.RequestMessage.RequestUri.OriginalString,
                    string.Format("{0} {1}", (int)response.StatusCode, response.StatusCode.ToString()),
                    response.ReasonPhrase,
                    Convert.ToDecimal(Convert.ToDouble(response.Content.Headers.ContentLength) / 1024),
                    response.Content?.ReadAsStringAsync().Result
                    );
                this.Log(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error during Log(HttpResponseMessage request): " + ex.ToString());
            }
        }

        #endregion

        #endregion

        protected class HttpConnectionSettings
        {
            public string Content { get; set; }
            public Encoding Encoding { get; set; }
            public string MediaType { get; set; }
        }
    }
}