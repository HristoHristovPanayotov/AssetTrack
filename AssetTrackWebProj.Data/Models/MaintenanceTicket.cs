using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetTrack.Data.Models
{
    /// <summary>
    /// A work order / repair intervention raised against a specific asset.
    /// </summary>
    public class MaintenanceTicket
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = null!;

        public DateTime DateReported { get; set; }

        public bool IsResolved { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RepairCost { get; set; }

        // FK -> Asset
        public int AssetId { get; set; }
        public Asset Asset { get; set; } = null!;
    }
}
