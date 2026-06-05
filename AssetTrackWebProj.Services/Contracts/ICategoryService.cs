using AssetTrack.Services.Models.Categories;
using AssetTrack.Services.Models.Common;

namespace AssetTrack.Services.Contracts
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryListItemViewModel>> GetAllAsync();

        Task<CategoryFormModel?> GetForEditAsync(int id);

        Task<int> CreateAsync(CategoryFormModel model);

        Task UpdateAsync(CategoryFormModel model);

        Task<IEnumerable<CategoryOption>> GetCategoryOptionsAsync();

        Task<bool> ExistsAsync(int id);
    }
}
