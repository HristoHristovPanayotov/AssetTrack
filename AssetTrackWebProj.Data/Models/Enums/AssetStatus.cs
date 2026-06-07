namespace AssetTrack.Data.Models.Enums
{
    /// <summary>
    /// Lifecycle state of a physical corporate asset.
    /// </summary>
    public enum AssetStatus
    {
        Available = 0,
        Assigned = 1,
        UnderMaintenance = 2,
        Retired = 3
    }
}
