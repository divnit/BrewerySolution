using Brewery.Application.DTOs;
using Brewery.Application.Interfaces;
using Brewery.Core.Utilities;

namespace Brewery.Application.Services
{
    public class BreweryService : IBreweryService
    {
        private readonly IBreweryRepository _repo;
        private const int MaxPageSize = 200;

        public BreweryService(IBreweryRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResult<BreweryDto>> GetAllAsync(string? q, string? sortBy, bool desc, double? lat, double? lon, int page, int pageSize, CancellationToken ct = default)
        {
            var items = await _repo.GetAllAsync(ct);
            IEnumerable<Core.Entities.Brewery> filtered = items;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim();
                filtered = filtered.Where(b =>
                    (!string.IsNullOrWhiteSpace(b.Name) && b.Name.Contains(t, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(b.City) && b.City.Contains(t, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(b.Phone) && b.Phone.Contains(t, StringComparison.OrdinalIgnoreCase)));
            }

            var dtos = filtered.Select(b => MapToDto(b, lat, lon));

            dtos = (sortBy ?? "name").ToLowerInvariant() switch
            {
                "city" => desc ? dtos.OrderByDescending(d => d.City) : dtos.OrderBy(d => d.City),
                "distance" => desc ? dtos.OrderByDescending(d => d.DistanceKm ?? double.MaxValue) : dtos.OrderBy(d => d.DistanceKm ?? double.MaxValue),
                _ => desc ? dtos.OrderByDescending(d => d.Name) : dtos.OrderBy(d => d.Name)
            };

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
            var total = dtos.Count();
            var pageItems = dtos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<BreweryDto>(pageItems, total, page, pageSize);
        }

        public async Task<BreweryDto?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var b = await _repo.GetByIdAsync(id, ct);
            return b is null ? null : MapToDto(b, null, null);
        }

        public async Task<IReadOnlyList<string>> AutocompleteAsync(string term, int take = 10, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(term)) return Array.Empty<string>();
            var results = await _repo.SearchAsync(term, ct);
            var names = results.Select(r => r.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .Take(Math.Clamp(take, 1, 50))
                .ToList();
            return names;
        }

        public async Task<BreweryDto?> GetRandomAsync(CancellationToken ct = default)
        {
            var b = await _repo.GetRandomAsync(ct);
            return b is null ? null : MapToDto(b, null, null);
        }

        private static BreweryDto MapToDto(Core.Entities.Brewery b, double? userLat, double? userLon)
        {
            double? distance = null;
            if (userLat.HasValue && userLon.HasValue && b.Latitude.HasValue && b.Longitude.HasValue)
            {
                distance = DistanceCalculator.HaversineKm(userLat.Value, userLon.Value, b.Latitude.Value, b.Longitude.Value);
            }

            return new BreweryDto
            {
                Id = b.Id,
                Name = b.Name,
                City = b.City,
                Phone = b.Phone,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                DistanceKm = distance
            };
        }
    } 
}
