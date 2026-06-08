using AssetTrack.Services.Models.Common;

namespace AssetTrack.Services.Models.Assets
{
    /// <summary>
    /// Backing model for the asset master grid: paged items plus the current
    /// search term and sort state (used to build sort/paging links in the view).
    /// </summary>
    public class AssetIndexViewModel
    {
        public PaginatedList<AssetListItemViewModel> Assets { get; set; } = null!;
        public string? SearchTerm { get; set; }
        public string? SortOrder { get; set; }

        public string NameSortParam => string.IsNullOrEmpty(SortOrder) ? "name_desc" : "";
        public string SerialSortParam => SortOrder == "serial" ? "serial_desc" : "serial";
        public string ValueSortParam => SortOrder == "value" ? "value_desc" : "value";
    }
}
