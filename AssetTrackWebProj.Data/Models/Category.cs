using System.ComponentModel.DataAnnotations;

namespace AssetTrack.Data.Models
{
    /// <summary>
    /// Logical grouping of corporate inventory (e.g. Laptops, Monitors, Servers).
    /// One category maps to many assets.
    /// </summary>
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public ICollection<Asset> Assets { get; set; } = new HashSet<Asset>();
    }
}
