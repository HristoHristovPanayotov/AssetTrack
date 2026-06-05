using AssetTrack.Services.Models.Assets;
using AssetTrack.Services.Models.Common;

namespace AssetTrack.Services.Contracts
{
    public interface IAssetService
    {
        Task<AssetIndexViewModel> GetAssetsAsync(
            string? searchTerm, string? sortOrder, int pageIndex, int pageSize);

        Task<AssetDetailsViewModel?> GetDetailsAsync(int id);

        Task<AssetFormModel?> GetForEditAsync(int id);

        Task<int> CreateAsync(AssetFormModel model);

        Task UpdateAsync(AssetFormModel model);

        Task DeleteAsync(int id);

        Task AssignToUserAsync(int assetId, string userId);

        Task UnassignAsync(int assetId);

        Task<IEnumerable<AssetListItemViewModel>> GetAssetsForUserAsync(string userId);

        Task<IEnumerable<AssetListItemViewModel>> GetAllSimpleAsync();

        Task<int> GetTotalAssetCountAsync();

        Task<decimal> GetTotalInventoryValueAsync();

        Task<bool> ExistsAsync(int id);
    }
}
