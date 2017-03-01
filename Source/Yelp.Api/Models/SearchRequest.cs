using Newtonsoft.Json;

namespace Yelp.Api.Models
{
    /// <summary>
    /// Container class for all parameters used by the Search API.
    /// </summary>
    public sealed class SearchRequest : TrackedChangesModelBase, ICoordinates
    {
        #region Properties

        private string _Term;
        /// <summary>
        /// Optional. Search term (e.g. "food", "restaurants"). If term isn’t included we search everything. The term keyword also 
        /// accepts business names such as "Starbucks".
        /// </summary>
        [JsonProperty("term")]
        public string Term
        {
            get { return _Term; }
            set { this.SetProperty(ref _Term, value); }
        }

        private string _Location;
        /// <summary>
        /// Required if either latitude or longitude is not provided. Specifies the combination of "address, neighborhood, city, state or 
        /// zip, optional country" to be used when searching for businesses.
        /// </summary>
        [JsonProperty("location")]
        public string Location
        {
            get { return _Location; }
            set { this.SetProperty(ref _Location, value); }
        }

        private double _Latitude = double.NaN;
        /// <summary>
        /// Required if location is not provided. Latitude of the location you want to search near by. 
        /// </summary>
        [JsonProperty("latitude")]
        public double Latitude
        {
            get { return _Latitude; }
            set { this.SetProperty(ref _Latitude, value); }
        }

        private double _Longitude = double.NaN;
        /// <summary>
        /// Required if location is not provided. Longitude of the location you want to search near by.
        /// </summary>
        [JsonProperty("longitude")]
        public double Longitude
        {
            get { return _Longitude; }
            set { this.SetProperty(ref _Longitude, value); }
        }

        private int _Radius;
        /// <summary>
        /// Optional. Search radius in meters. If the value is too large, a AREA_TOO_LARGE error may be returned. The max value is 40000 meters (25 miles).
        /// </summary>
        [JsonProperty("radius")]
        public int Radius
        {
            get { return _Radius; }
            set { this.SetProperty(ref _Radius, value); }
        }

        private string _Categories;
        /// <summary>
        /// Optional. Categories to filter the search results with. See the list of supported categories. The category filter can be a list of 
        /// comma delimited categories. For example, "bars,french" will filter by Bars and French. The category identifier should be used 
        /// (for example "discgolf", not "Disc Golf").
        /// </summary>
        [JsonProperty("categories")]
        public string Categories
        {
            get { return _Categories; }
            set { this.SetProperty(ref _Categories, value); }
        }

        private string _Locale;
        /// <summary>
        /// Optional. Specify the locale to return the business information in. See the list of supported locales at https://www.yelp.com/developers/documentation/v3/supported_locales
        /// </summary>
        [JsonProperty("locale")]
        public string Locale
        {
            get { return _Locale; }
            set { this.SetProperty(ref _Locale, value); }
        }

        private int _MaxResults = 20;
        /// <summary>
        /// Optional. Number of business results to return. By default, it will return 20. Maximum is 50.
        /// </summary>
        [JsonProperty("limit")]
        public int MaxResults
        {
            get { return _MaxResults; }
            set { this.SetProperty(ref _MaxResults, value); }
        }

        private int _ResultsOffset = 0;
        /// <summary>
        /// Optional. Offset the list of returned business results by this amount.
        /// </summary>
        [JsonProperty("offset")]
        public int ResultsOffset
        {
            get { return _ResultsOffset; }
            set { this.SetProperty(ref _ResultsOffset, value); }
        }

        private string _SortBy;
        /// <summary>
        /// Optional. Sort the results by one of the these modes: best_match, rating, review_count or distance. By default it's best_match. The rating 
        /// sort is not strictly sorted by the rating value, but by an adjusted rating value that takes into account the number of ratings, similar 
        /// to a bayesian average. This is so a business with 1 rating of 5 stars doesn’t immediately jump to the top.
        /// </summary>
        [JsonProperty("sort_by")]
        public string SortBy
        {
            get { return _SortBy; }
            set { this.SetProperty(ref _SortBy, value); }
        }

        private string _Price;
        /// <summary>
        /// Optional. Pricing levels to filter the search result with: 1 = $, 2 = $$, 3 = $$$, 4 = $$$$. The price filter can be a list of comma delimited 
        /// pricing levels. For example, "1, 2, 3" will filter the results to show the ones that are $, $$, or $$$.
        /// </summary>
        [JsonProperty("price")]
        public string Price
        {
            get { return _Price; }
            set { this.SetProperty(ref _Price, value); }
        }

        private bool _OpenNow;
        /// <summary>
        /// Optional. Default to false. When set to true, only return the businesses open now. Notice that open_at and open_now cannot be used together.
        /// </summary>
        [JsonProperty("open_now")]
        public bool OpenNow
        {
            get { return _OpenNow; }
            set { this.SetProperty(ref _OpenNow, value); }
        }

        private int _OpenAt;
        /// <summary>
        /// Optional. An integer represending the Unix time in the same timezone of the search location. If specified, it will return business open at the given 
        /// time. Notice that open_at and open_now cannot be used together.
        /// </summary>
        [JsonProperty("open_at")]
        public int OpenAt
        {
            get { return _OpenAt; }
            set { this.SetProperty(ref _OpenAt, value); }
        }

        private string _Attributes;
        /// <summary>
        /// Optional. Additional filters to restrict search results. Possible values are: 
        ///     hot_and_new - Hot and New businesses
        ///     request_a_quote - Businesses that have the Request a Quote feature
        ///     waitlist_reservation - Businesses that have an online waitlist
        ///     cashback - Businesses that offer Cash Back
        ///     deals - Businesses that offer Deals
        /// You can combine multiple attributes by providing a comma separated like "attribute1,attribute2". If multiple attributes are used, only businesses that 
        /// satisfy ALL attributes will be returned in search results.For example, the attributes "hot_and_new,cashback" will return businesses that are 
        /// Hot and New AND offer Cash Back. 
        /// </summary>
        [JsonProperty("attributes")]
        public string Attributes
        {
            get { return _Attributes; }
            set { this.SetProperty(ref _Attributes, value); }
        }

        #endregion
    }
}