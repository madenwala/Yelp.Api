#Yelp.API
C# Portable Class Library for Yelp (v3) Fusion API works with Windows 10, Windows 8, Windows Phone, ASP.NET, Xamarin (iOS/Android), Mac, and other .NET projects. Yelp's (v3) Fusion API allows you to get the best local business information and user reviews of over million businesses in 32 countries.

## How to use the Yelp API in your .NET based app

Integrating this API is very easy.

1. **[Register](https://www.yelp.com/developers/v3/manage_app)** yourself with Yelp's developer program
2. Add the **[Yelp API client from NuGet](https://www.nuget.org/packages/Yelp.Api/)** to your project or register in the NuGet console with:

	`Install-Package Yelp.Api`

3. Call the API via the `Client` object in your code. See below for examples:

```c#
    var client = new Yelp.Api.Client("APP_ID", "APP_SECRET");
    var results = await client.SearchBusinessesAllAsync("cupcakes", 37.786882, -122.399972);
```

or if you want to perform a more advanced search, use the `SearchParameters` object.

```c#
    var request = new Yelp.Api.Models.SearchRequest();
    request.Latitude = 37.786882;
    request.Longitude = -122.399972;
    request.Term = "cupcakes";
    request.MaxResults = 40;
    request.OpenNow = true;

    var client = new Yelp.Api.Client("{APP_ID}", "{APP_SECRET}");
    var results = await client.SearchBusinessesAllAsync(request);
```
