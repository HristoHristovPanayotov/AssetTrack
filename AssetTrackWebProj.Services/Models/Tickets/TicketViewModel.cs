namespace AssetTrack.Services.Models.Tickets
{
    /// <summary>
    /// Serialised to JSON by the Web API controller and consumed by the AJAX
    /// fetch() call on the Asset Details page.
    /// </summary>
    public class TicketViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public DateTime DateReported { get; set; }
        public bool IsResolved { get; set; }
        public decimal RepairCost { get; set; }
        public int AssetId { get; set; }
    }
}
