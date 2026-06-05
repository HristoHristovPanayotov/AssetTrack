namespace AssetTrack.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalAssets { get; set; }
        public int OpenTickets { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public decimal TotalRepairSpend { get; set; }
    }
}
