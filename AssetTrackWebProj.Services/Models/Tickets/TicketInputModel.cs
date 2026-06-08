using System.ComponentModel.DataAnnotations;
using AssetTrack.Services.Models.Assets;

namespace AssetTrack.Services.Models.Tickets
{
    public class TicketInputModel
    {
        [Required(ErrorMessage = "Please choose the asset that is malfunctioning.")]
        [Display(Name = "Asset")]
        public int AssetId { get; set; }

        [Required(ErrorMessage = "Please describe the problem.")]
        [StringLength(1000, MinimumLength = 5,
            ErrorMessage = "Description must be between {2} and {1} characters.")]
        public string Description { get; set; } = null!;

        // Populated for the dropdown when the form is rendered.
        public IEnumerable<AssetListItemViewModel> AvailableAssets { get; set; }
            = new List<AssetListItemViewModel>();
    }
}
