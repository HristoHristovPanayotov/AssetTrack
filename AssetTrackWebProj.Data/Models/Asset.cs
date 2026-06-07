using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssetTrack.Data.Models.Enums;

namespace AssetTrack.Data.Models
{
    /// <summary>
    /// A physical piece of corporate hardware that is tracked by the system.
    /// </summary>
    public class Asset
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Model { get; set; } = null!;

        public DateTime PurchaseDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        public AssetStatus Status { get; set; } = AssetStatus.Available;

        // FK -> Category (one-to-many)
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // FK -> ApplicationUser (nullable, one-to-many)
        public string? AssignedUserId { get; set; }
        public ApplicationUser? AssignedUser { get; set; }

        public ICollection<MaintenanceTicket> MaintenanceTickets { get; set; }
            = new HashSet<MaintenanceTicket>();
    }
}
