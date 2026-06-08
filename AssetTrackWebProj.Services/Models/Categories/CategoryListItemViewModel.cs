namespace AssetTrack.Services.Models.Categories
{
    public class CategoryListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int AssetCount { get; set; }
    }
}
