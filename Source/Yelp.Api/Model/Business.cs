using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yelp.Api.Model
{
    public class BusinessList : ResponseBase
    {
        public Region region { get; set; }
        public int total { get; set; }
        public Business[] businesses { get; set; }
    }

    public class Region
    {
        public Center center { get; set; }
    }

    public class Center
    {
        public float longitude { get; set; }
        public float latitude { get; set; }
    }

    public class Business
    {
        public Category[] categories { get; set; }
        public string image_url { get; set; }
        public string phone { get; set; }
        public string price { get; set; }
        public string url { get; set; }
        public float rating { get; set; }
        public Coordinates coordinates { get; set; }
        public string display_phone { get; set; }
        public string name { get; set; }
        public Location location { get; set; }
        public float distance { get; set; }
        public bool is_closed { get; set; }
        public string id { get; set; }
        public int review_count { get; set; }
    }

    public class Coordinates
    {
        public float longitude { get; set; }
        public float latitude { get; set; }
    }

    public class Location
    {
        public string country { get; set; }
        public string address2 { get; set; }
        public string address1 { get; set; }
        public string state { get; set; }
        public string zip_code { get; set; }
        public string address3 { get; set; }
        public string[] display_address { get; set; }
        public string city { get; set; }
    }

    public class Category
    {
        public string alias { get; set; }
        public string title { get; set; }
    }
}