using Brewery.Persistence.Data;
using Brewery.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Brewery.Persistence.Repositories
{
    public class EfBreweryRepository : Application.Interfaces.IBreweryRepository
    {
        private readonly BreweryDbContext _db;

        public EfBreweryRepository(BreweryDbContext db)
        {
            _db = db;
        }

        private static Core.Entities.Brewery Map(BreweryEntity e) => new()
        {
            Id = e.Id,
            Name = e.Name,
            BreweryType = e.BreweryType,
            Street = e.Street,
            City = e.City,
            State = e.State,
            PostalCode = e.PostalCode,
            Country = e.Country,
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            Phone = e.Phone,
            WebsiteUrl = e.WebsiteUrl
        };

        public async Task<IReadOnlyList<Brewery.Core.Entities.Brewery>> GetAllAsync(CancellationToken ct = default)
        {
            var rows = await _db.Breweries.AsNoTracking().ToListAsync(ct);
            return rows.Select(Map).ToList();
        }

        public async Task<Brewery.Core.Entities.Brewery?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var e = await _db.Breweries.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct);
            return e is null ? null : Map(e);
        }

        public async Task<IReadOnlyList<Brewery.Core.Entities.Brewery>> SearchAsync(string query, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query)) return Array.Empty<Brewery.Core.Entities.Brewery>();
            var q = query.Trim();
            var rows = await _db.Breweries
                .AsNoTracking()
                .Where(b => EF.Functions.Like(b.Name, $"%{q}%") || EF.Functions.Like(b.City, $"%{q}%"))
                .OrderBy(b => b.Name)
                .Take(200)
                .ToListAsync(ct);

            return rows.Select(Map).ToList();
        }

        public async Task<Brewery.Core.Entities.Brewery?> GetRandomAsync(CancellationToken ct = default)
        {
            var e = await _db.Breweries.AsNoTracking().OrderBy(b => Guid.NewGuid()).FirstOrDefaultAsync(ct);
            return e is null ? null : Map(e);
        }
    }
}