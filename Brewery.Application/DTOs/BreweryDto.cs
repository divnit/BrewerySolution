using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brewery.Application.DTOs
{
    public sealed record BreweryDto
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string? Phone { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public double? DistanceKm { get; init; }
    }
}
