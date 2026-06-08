namespace AssetTrack.Services.Models.Common
{
    /// <summary>
    /// Lightweight key/value pair used to populate category dropdowns in the UI
    /// without leaking MVC SelectList types into the service layer.
    /// </summary>
    public class CategoryOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
