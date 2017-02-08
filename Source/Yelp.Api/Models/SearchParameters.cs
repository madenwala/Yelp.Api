using Newtonsoft.Json;

namespace Yelp.Api.Models
{
    public sealed class SearchParameters : TrackedChangesModelBase
    {
        #region Properties

        private string _Term;
        [JsonProperty("term")]
        public string Term
        {
            get { return _Term; }
            set { this.SetProperty(ref _Term, value); }
        }

        private string _Location;
        [JsonProperty("location")]
        public string Location
        {
            get { return _Location; }
            set { this.SetProperty(ref _Location, value); }
        }

        private double _Latitude;
        [JsonProperty("latitude")]
        public double Latitude
        {
            get { return _Latitude; }
            set { this.SetProperty(ref _Latitude, value); }
        }

        private double _Longitude;
        [JsonProperty("longitude")]
        public double Longitude
        {
            get { return _Longitude; }
            set { this.SetProperty(ref _Longitude, value); }
        }

        private int _Radius;
        [JsonProperty("radius")]
        public int Radius
        {
            get { return _Radius; }
            set { this.SetProperty(ref _Radius, value); }
        }

        private string _Categories;
        [JsonProperty("categories")]
        public string Categories
        {
            get { return _Categories; }
            set { this.SetProperty(ref _Categories, value); }
        }

        private string _Locale;
        [JsonProperty("locale")]
        public string Locale
        {
            get { return _Locale; }
            set { this.SetProperty(ref _Locale, value); }
        }

        private int _MaxResults;
        [JsonProperty("limit")]
        public int MaxResults
        {
            get { return _MaxResults; }
            set { this.SetProperty(ref _MaxResults, value); }
        }

        private int _ResultsOffset;
        [JsonProperty("offset")]
        public int ResultsOffset
        {
            get { return _ResultsOffset; }
            set { this.SetProperty(ref _ResultsOffset, value); }
        }

        private string _SortBy;
        [JsonProperty("sort_by")]
        public string SortBy
        {
            get { return _SortBy; }
            set { this.SetProperty(ref _SortBy, value); }
        }

        private string _Price;
        [JsonProperty("price")]
        public string Price
        {
            get { return _Price; }
            set { this.SetProperty(ref _Price, value); }
        }

        private bool _OpenNow;
        [JsonProperty("open_now")]
        public bool OpenNow
        {
            get { return _OpenNow; }
            set { this.SetProperty(ref _OpenNow, value); }
        }

        private int _OpenAt;
        [JsonProperty("open_at")]
        public int OpenAt
        {
            get { return _OpenAt; }
            set { this.SetProperty(ref _OpenAt, value); }
        }

        private string _Attributes;
        [JsonProperty("attributes")]
        public string Attributes
        {
            get { return _Attributes; }
            set { this.SetProperty(ref _Attributes, value); }
        }

        #endregion
    }
}