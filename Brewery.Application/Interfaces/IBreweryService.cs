using Brewery.Application.DTOs;

namespace Brewery.Application.Interfaces
{
    public interface IBreweryService
    {
        Task<PagedResult<BreweryDto>> GetAllAsync(string? q, string? sortBy, bool desc, double? lat, double? lon, int page, int pageSize, CancellationToken ct = default);
        
        Task<BreweryDto?> GetByIdAsync(string id, CancellationToken ct = default);
        
        Task<IReadOnlyList<string>> AutocompleteAsync(string term, int take = 10, CancellationToken ct = default);
        
        Task<BreweryDto?> GetRandomAsync(CancellationToken ct = default);
    }
}
