namespace Yelp.Api.Models
{
    public class ConnectionRetrySettings
    {
        public ConnectionRetrySettings(int currentTry = 1, bool isRetryConnections = true, int maxAmountOfTries = 10)
        {
            CurrentTry = currentTry;
            IsRetryConnections = isRetryConnections;
            MaxAmountOfTries = maxAmountOfTries;
        }

        /// <summary>
        /// This is the current try number of the request.
        /// </summary>
        public int CurrentTry { get; set; }

        /// <summary>
        /// This sets whether the request should make retry attempts.
        /// <c>true</c> Yes, attempt retries.
        /// <c>false</c> No, do not attempt retries.
        /// </summary>
        public bool IsRetryConnections { get; set; }

        /// <summary>
        /// The maximum amount of tries you want this request to make.
        /// </summary>
        public int MaxAmountOfTries { get; set; }
    }
}
