using AssetTrack.Data.Models.Enums;

namespace AssetTrack.Services.Models.Assets
{
    public class AssetDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string SerialNumber { get; set; } = null!;
        public string Model { get; set; } = null!;
        public DateTime PurchaseDate { get; set; }
        public decimal Value { get; set; }
        public AssetStatus Status { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? AssignedUserName { get; set; }
        public string? AssignedUserId { get; set; }
    }
}
