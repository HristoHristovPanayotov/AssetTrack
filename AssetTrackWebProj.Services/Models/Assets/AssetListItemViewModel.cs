using AssetTrack.Data.Models.Enums;

namespace AssetTrack.Services.Models.Assets
{
    public class AssetListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string SerialNumber { get; set; } = null!;
        public string Model { get; set; } = null!;
        public decimal Value { get; set; }
        public AssetStatus Status { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? AssignedUserName { get; set; }
    }
}
