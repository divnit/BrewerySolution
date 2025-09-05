using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brewery.Persistence.Entities
{
    [Table("Breweries")]
    public class BreweryEntity
    {
        [Key]
        public string Id { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? BreweryType { get; set; }
        public string? Street { get; set; }
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Phone { get; set; }
        public string? WebsiteUrl { get; set; }
    }
}
