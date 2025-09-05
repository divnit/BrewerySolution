using Brewery.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brewery.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BreweriesController : ControllerBase
    {
        private readonly IBreweryService _svc;

        public BreweriesController(IBreweryService svc)
        {
            _svc = svc;
        }

        // GET: api/v1/breweries
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? q,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] double? lat = null,
            [FromQuery] double? lon = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            try
            {
                var res = await _svc.GetAllAsync(q, sortBy, desc, lat, lon, page, pageSize, ct);
                return Ok(res);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/v1/breweries/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct = default)
        {
            var dto = await _svc.GetByIdAsync(id, ct);
            return dto == null ? NotFound() : Ok(dto);
        }

        // GET: api/v1/breweries/autocomplete?term=xyz
        [HttpGet("autocomplete")]
        public async Task<IActionResult> Autocomplete([FromQuery] string term, [FromQuery] int take = 10, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { error = "term is required" });

            var items = await _svc.AutocompleteAsync(term, take, ct);
            return Ok(items);
        }

        // GET: api/v1/breweries/random
        [HttpGet("random")]
        public async Task<IActionResult> Random(CancellationToken ct = default)
        {
            var dto = await _svc.GetRandomAsync(ct);
            return dto == null ? NotFound() : Ok(dto);
        }
    }
}
