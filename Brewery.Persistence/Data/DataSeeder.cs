using Brewery.Persistence.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brewery.Persistence.Data
{
    public class DataSeeder
    {
        private readonly BreweryDbContext _db;
        private readonly IOpenBreweryClient _client;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(BreweryDbContext db, IOpenBreweryClient client, ILogger<DataSeeder> logger)
        {
            _db = db;
            _client = client;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken ct = default)
        {
            await _db.Database.MigrateAsync(ct);

            if (await _db.Breweries.AnyAsync(ct))
            {
                _logger.LogInformation("DB already has data; skipping seed.");
                return;
            }

            _logger.LogInformation("Seeding breweries from OpenBrewery...");
            var items = await _client.FetchAllAsync(ct);
            if (items.Any())
            {
                await _db.Breweries.AddRangeAsync(items, ct);
                await _db.SaveChangesAsync(ct);
                _logger.LogInformation("Seeded {Count} breweries", items.Count);
            }
        }
    }
}
