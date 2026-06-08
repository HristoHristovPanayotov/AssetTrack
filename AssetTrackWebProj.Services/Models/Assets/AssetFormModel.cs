using System.ComponentModel.DataAnnotations;
using AssetTrack.Data.Models.Enums;
using AssetTrack.Services.Models.Common;

namespace AssetTrack.Services.Models.Assets
{
    /// <summary>
    /// Input model used by both Create and Edit. Carries explicit Data Annotation
    /// validation rules that drive both client-side (jQuery) and server-side checks.
    /// </summary>
    public class AssetFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Asset name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Name must be between {2} and {1} characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Serial number is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Serial number must be between {2} and {1} characters.")]
        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; } = null!;

        [Required(ErrorMessage = "Model is required.")]
        [StringLength(100, MinimumLength = 1)]
        public string Model { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;

        [Required]
        [Range(0.0, 1_000_000.0,
            ErrorMessage = "Value must be a positive amount up to 1,000,000.")]
        [DataType(DataType.Currency)]
        public decimal Value { get; set; }

        [Required]
        public AssetStatus Status { get; set; } = AssetStatus.Available;

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Assigned Employee")]
        public string? AssignedUserId { get; set; }

        // Populated by the controller for the dropdowns; not bound on POST.
        public IEnumerable<CategoryOption> Categories { get; set; } = new List<CategoryOption>();
        public IEnumerable<UserOption> Users { get; set; } = new List<UserOption>();
    }
}
