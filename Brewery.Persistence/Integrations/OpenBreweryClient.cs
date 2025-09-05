using Brewery.Persistence.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Brewery.Persistence.Integrations
{
    public class OpenBreweryClient : IOpenBreweryClient
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly ILogger<OpenBreweryClient> _logger;
        private readonly int _cacheMinutes;
        private const string CacheKey = "openbrewery:all";

        public OpenBreweryClient(HttpClient http, IMemoryCache cache, ILogger<OpenBreweryClient> logger, IConfiguration cfg)
        {
            _http = http;
            _cache = cache;
            _logger = logger;
            _cacheMinutes = cfg.GetValue<int?>("OpenBrewery:CacheMinutes") ?? 10;
        }

        private class SourceItem
        {
            public string id { get; set; } = string.Empty;
            public string name { get; set; } = string.Empty;
            public string? brewery_type { get; set; }
            public string? street { get; set; }
            public string? city { get; set; }
            public string? state { get; set; }
            public string? postal_code { get; set; }
            public string? country { get; set; }
            public double? longitude { get; set; }
            public double? latitude { get; set; }
            public string? phone { get; set; }
            public string? website_url { get; set; }
        }

        private static BreweryEntity Map(SourceItem s) => new()
        {
            Id = s.id,
            Name = s.name,
            BreweryType = s.brewery_type,
            Street = s.street,
            City = s.city ?? string.Empty,
            State = s.state,
            PostalCode = s.postal_code,
            Country = s.country,
            Longitude = double.TryParse(Convert.ToString(s.longitude), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lon) ? lon : (double?)null,
            Latitude = double.TryParse(Convert.ToString(s.latitude), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lat) ? lat : (double?)null,
            Phone = s.phone,
            WebsiteUrl = s.website_url
        };

        public async Task<IReadOnlyList<BreweryEntity>> FetchAllAsync(CancellationToken ct = default)
        {
            if (_cache.TryGetValue(CacheKey, out IReadOnlyList<BreweryEntity>? cached) && cached is not null)
            {
                _logger.LogInformation("Returning breweries from cache ({Count})", cached.Count);
                return cached;
            }

            var url = $"breweries?per_page=200";
            _logger.LogInformation("Fetching breweries from {Url}", url);
            var data = await _http.GetFromJsonAsync<List<SourceItem>>(url, ct) ?? new List<SourceItem>();
            var mapped = data.Select(Map).ToList();
            _cache.Set(CacheKey, mapped, TimeSpan.FromMinutes(_cacheMinutes));
            return mapped;
        }

        public async Task<BreweryEntity?> FetchByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                var item = await _http.GetFromJsonAsync<SourceItem>($"breweries/{id}", ct);
                return item is null ? null : Map(item);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<IReadOnlyList<BreweryEntity>> SearchAsync(string query, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query)) return Array.Empty<BreweryEntity>();
            var url = $"breweries/search?query={Uri.EscapeDataString(query)}";
            var data = await _http.GetFromJsonAsync<List<SourceItem>>(url, ct) ?? new List<SourceItem>();
            return data.Select(Map).ToList();
        }

        public async Task<BreweryEntity?> FetchRandomAsync(CancellationToken ct = default)
        {
            var data = await _http.GetFromJsonAsync<List<SourceItem>>("breweries/random", ct) ?? new List<SourceItem>();
            var first = data.FirstOrDefault();
            return first is null ? null : Map(first);
        }
    } 
}
