using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        /// <param name="url">URL to retrieve data from.</param>should be deserialized.</param>
        /// <param name="retryCount">Number of retry attempts if a call fails. Default is zero.</param>
        /// <param name="serializerType">Specifies how the data should be deserialized.</param>
        /// <returns>Instance of the type specified representing the data returned from the URL.</returns>
        /// <summary>
        protected async Task<T> GetAsync<T>(string url, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var response = await this.Client.GetAsync(new Uri(this.BaseUri, url), ct);
            this.Log(response);
            var data = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>
        /// Posts data to the specified URL.
        /// </summary>
        /// <typeparam name="T">Type for the strongly typed class representing data returned from the URL.</typeparam>
        /// <param name="url">URL to retrieve data from.</param>
        /// <param name="contents">Any content that should be passed into the post.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <param name="serializerType">Specifies how the data should be deserialized.</param>
        /// <returns>Instance of the type specified representing the data returned from the URL.</returns>
        protected async Task<T> PostAsync<T>(string url, CancellationToken ct, HttpContent contents = default(HttpContent))
        {
            string data = await this.PostAsync(url, ct, contents);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>
        /// Posts data to the specified URL.
        /// </summary>
        /// <param name="url">URL to retrieve data from.</param>
        /// <param name="contents">Any content that should be passed into the post.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <param name="serializerType">Specifies how the data should be deserialized.</param>
        /// <returns>Response contents as string else null if nothing.</returns>
        protected async Task<string> PostAsync(string url, CancellationToken ct, HttpContent contents = default(HttpContent))
        {
            HttpResponseMessage response = await this.PostAsync(url, contents, ct);
            var data = await response.Content?.ReadAsStringAsync();
            return data;
        }

        /// <summary>
        /// Posts data to the specified URL.
        /// </summary>
        /// <param name="url">URL to retrieve data from.</param>
        /// <param name="contents">Any content that should be passed into the post.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <param name="serializerType">Specifies how the data should be deserialized.</param>
        /// <returns>Response contents as string else null if nothing.</returns>
        protected async Task<HttpResponseMessage> PostAsync(string url, HttpContent contents, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var response = await this.Client.PostAsync(new Uri(this.BaseUri, url), contents, ct);
            this.Log(response);
            return response;
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
                    request.Headers?.ToString(),
                    request.Content?.ReadAsStringAsync().Result
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
    }
}