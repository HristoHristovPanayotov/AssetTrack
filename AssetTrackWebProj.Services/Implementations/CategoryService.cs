using AssetTrack.Data.Models;
using AssetTrack.Services.Contracts;
using AssetTrack.Services.Exceptions;
using AssetTrack.Services.Models.Categories;
using AssetTrack.Services.Models.Common;
using AssetTrack.Services.Repository;
using Microsoft.EntityFrameworkCore;

namespace AssetTrack.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categories;

        public CategoryService(IRepository<Category> categories)
        {
            _categories = categories;
        }

        public async Task<IEnumerable<CategoryListItemViewModel>> GetAllAsync()
        {
            return await _categories
                .AllAsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryListItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    AssetCount = c.Assets.Count
                })
                .ToListAsync();
        }

        public async Task<CategoryFormModel?> GetForEditAsync(int id)
        {
            return await _categories
                .AllAsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CategoryFormModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(CategoryFormModel model)
        {
            await EnsureUniqueNameAsync(model.Name, null);

            var category = new Category
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim()
            };

            await _categories.AddAsync(category);
            await _categories.SaveChangesAsync();
            return category.Id;
        }

        public async Task UpdateAsync(CategoryFormModel model)
        {
            var category = await _categories.GetByIdAsync(model.Id)
                ?? throw new ServiceValidationException("The category could not be found.");

            await EnsureUniqueNameAsync(model.Name, model.Id);

            category.Name = model.Name.Trim();
            category.Description = model.Description?.Trim();

            _categories.Update(category);
            await _categories.SaveChangesAsync();
        }

        public async Task<IEnumerable<CategoryOption>> GetCategoryOptionsAsync()
        {
            return await _categories
                .AllAsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryOption { Id = c.Id, Name = c.Name })
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
            => await _categories.AllAsNoTracking().AnyAsync(c => c.Id == id);

        private async Task EnsureUniqueNameAsync(string name, int? ignoreId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ServiceValidationException("Category name is required.");
            }

            var trimmed = name.Trim();
            var exists = await _categories
                .AllAsNoTracking()
                .AnyAsync(c => c.Name == trimmed && (ignoreId == null || c.Id != ignoreId));

            if (exists)
            {
                throw new ServiceValidationException(
                    $"A category named '{trimmed}' already exists.");
            }
        }
    }
}
