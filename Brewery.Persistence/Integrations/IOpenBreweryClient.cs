using Brewery.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brewery.Persistence.Integrations
{
    public interface IOpenBreweryClient
    {
        Task<IReadOnlyList<BreweryEntity>> FetchAllAsync(CancellationToken ct = default);
        Task<BreweryEntity?> FetchByIdAsync(string id, CancellationToken ct = default);
        Task<IReadOnlyList<BreweryEntity>> SearchAsync(string query, CancellationToken ct = default);
        Task<BreweryEntity?> FetchRandomAsync(CancellationToken ct = default);
    }
}
