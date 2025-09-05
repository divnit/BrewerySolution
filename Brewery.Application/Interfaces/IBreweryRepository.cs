namespace Brewery.Application.Interfaces
{
    public interface IBreweryRepository
    {
        Task<IReadOnlyList<Core.Entities.Brewery>> GetAllAsync(CancellationToken ct = default);
        
        Task<Core.Entities.Brewery?> GetByIdAsync(string id, CancellationToken ct = default);
        
        Task<IReadOnlyList<Core.Entities.Brewery>> SearchAsync(string query, CancellationToken ct = default);
        
        Task<Core.Entities.Brewery?> GetRandomAsync(CancellationToken ct = default);
    }
}
