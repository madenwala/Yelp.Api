using System;
using System.Net.Http;

namespace Yelp.Api.Exceptions
{
    /// <summary>
    /// This exception is thrown when you have exceeded your daily allotment of Yelp connections.
    /// </summary>
    public class AccessLimitException : HttpRequestException
    {
        public AccessLimitException()
        {
        }

        public AccessLimitException(string message) : base(message)
        {
        }

        public AccessLimitException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
