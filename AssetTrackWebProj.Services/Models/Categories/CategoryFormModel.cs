using System.ComponentModel.DataAnnotations;

namespace AssetTrack.Services.Models.Categories
{
    public class CategoryFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Name must be between {2} and {1} characters.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed {1} characters.")]
        public string? Description { get; set; }
    }
}
