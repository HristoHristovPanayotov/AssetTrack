using AssetTrack.Data.Models;
using AssetTrack.Data.Models.Enums;
using AssetTrack.Services.Contracts;
using AssetTrack.Services.Exceptions;
using AssetTrack.Services.Models.Assets;
using AssetTrack.Services.Models.Common;
using AssetTrack.Services.Repository;
using Microsoft.EntityFrameworkCore;

namespace AssetTrack.Services.Implementations
{
    public class AssetService : IAssetService
    {
        private const decimal MaxAssetValue = 1_000_000m;

        private readonly IRepository<Asset> _assets;

        public AssetService(IRepository<Asset> assets)
        {
            _assets = assets;
        }

        public async Task<AssetIndexViewModel> GetAssetsAsync(
            string? searchTerm, string? sortOrder, int pageIndex, int pageSize)
        {
            IQueryable<Asset> query = _assets
                .AllAsNoTracking()
                .Include(a => a.Category)
                .Include(a => a.AssignedUser);

            // Dynamic live text search over Name OR SerialNumber.
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(a =>
                    a.Name.Contains(term) || a.SerialNumber.Contains(term));
            }

            // Column-based sorting.
            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(a => a.Name),
                "serial" => query.OrderBy(a => a.SerialNumber),
                "serial_desc" => query.OrderByDescending(a => a.SerialNumber),
                "value" => query.OrderBy(a => a.Value),
                "value_desc" => query.OrderByDescending(a => a.Value),
                _ => query.OrderBy(a => a.Name)
            };

            var projected = query.Select(a => new AssetListItemViewModel
            {
                Id = a.Id,
                Name = a.Name,
                SerialNumber = a.SerialNumber,
                Model = a.Model,
                Value = a.Value,
                Status = a.Status,
                CategoryName = a.Category.Name,
                AssignedUserName = a.AssignedUser == null
                    ? null
                    : a.AssignedUser.FirstName + " " + a.AssignedUser.LastName
            });

            var paged = await PaginatedList<AssetListItemViewModel>
                .CreateAsync(projected, pageIndex, pageSize);

            return new AssetIndexViewModel
            {
                Assets = paged,
                SearchTerm = searchTerm,
                SortOrder = sortOrder
            };
        }

        public async Task<AssetDetailsViewModel?> GetDetailsAsync(int id)
        {
            return await _assets
                .AllAsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AssetDetailsViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    SerialNumber = a.SerialNumber,
                    Model = a.Model,
                    PurchaseDate = a.PurchaseDate,
                    Value = a.Value,
                    Status = a.Status,
                    CategoryName = a.Category.Name,
                    AssignedUserId = a.AssignedUserId,
                    AssignedUserName = a.AssignedUser == null
                        ? null
                        : a.AssignedUser.FirstName + " " + a.AssignedUser.LastName
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AssetFormModel?> GetForEditAsync(int id)
        {
            return await _assets
                .AllAsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AssetFormModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    SerialNumber = a.SerialNumber,
                    Model = a.Model,
                    PurchaseDate = a.PurchaseDate,
                    Value = a.Value,
                    Status = a.Status,
                    CategoryId = a.CategoryId,
                    AssignedUserId = a.AssignedUserId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(AssetFormModel model)
        {
            await ValidateAsync(model, isUpdate: false);

            var asset = new Asset
            {
                Name = model.Name.Trim(),
                SerialNumber = model.SerialNumber.Trim(),
                Model = model.Model.Trim(),
                PurchaseDate = model.PurchaseDate,
                Value = model.Value,
                Status = model.Status,
                CategoryId = model.CategoryId,
                AssignedUserId = string.IsNullOrWhiteSpace(model.AssignedUserId)
                    ? null
                    : model.AssignedUserId
            };

            // Keep status consistent with assignment.
            if (asset.AssignedUserId != null && asset.Status == AssetStatus.Available)
            {
                asset.Status = AssetStatus.Assigned;
            }

            await _assets.AddAsync(asset);
            await _assets.SaveChangesAsync();
            return asset.Id;
        }

        public async Task UpdateAsync(AssetFormModel model)
        {
            var asset = await _assets.GetByIdAsync(model.Id)
                ?? throw new ServiceValidationException("The asset could not be found.");

            await ValidateAsync(model, isUpdate: true);

            asset.Name = model.Name.Trim();
            asset.SerialNumber = model.SerialNumber.Trim();
            asset.Model = model.Model.Trim();
            asset.PurchaseDate = model.PurchaseDate;
            asset.Value = model.Value;
            asset.Status = model.Status;
            asset.CategoryId = model.CategoryId;
            asset.AssignedUserId = string.IsNullOrWhiteSpace(model.AssignedUserId)
                ? null
                : model.AssignedUserId;

            _assets.Update(asset);
            await _assets.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var asset = await _assets.GetByIdAsync(id)
                ?? throw new ServiceValidationException("The asset could not be found.");

            _assets.Delete(asset);
            await _assets.SaveChangesAsync();
        }

        public async Task AssignToUserAsync(int assetId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ServiceValidationException("A valid employee must be supplied.");
            }

            var asset = await _assets.GetByIdAsync(assetId)
                ?? throw new ServiceValidationException("The asset could not be found.");

            asset.AssignedUserId = userId;
            asset.Status = AssetStatus.Assigned;

            _assets.Update(asset);
            await _assets.SaveChangesAsync();
        }

        public async Task UnassignAsync(int assetId)
        {
            var asset = await _assets.GetByIdAsync(assetId)
                ?? throw new ServiceValidationException("The asset could not be found.");

            asset.AssignedUserId = null;
            if (asset.Status == AssetStatus.Assigned)
            {
                asset.Status = AssetStatus.Available;
            }

            _assets.Update(asset);
            await _assets.SaveChangesAsync();
        }

        public async Task<IEnumerable<AssetListItemViewModel>> GetAssetsForUserAsync(string userId)
        {
            return await _assets
                .AllAsNoTracking()
                .Include(a => a.Category)
                .Where(a => a.AssignedUserId == userId)
                .OrderBy(a => a.Name)
                .Select(a => new AssetListItemViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    SerialNumber = a.SerialNumber,
                    Model = a.Model,
                    Value = a.Value,
                    Status = a.Status,
                    CategoryName = a.Category.Name
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetListItemViewModel>> GetAllSimpleAsync()
        {
            return await _assets
                .AllAsNoTracking()
                .Include(a => a.Category)
                .OrderBy(a => a.Name)
                .Select(a => new AssetListItemViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    SerialNumber = a.SerialNumber,
                    Model = a.Model,
                    Value = a.Value,
                    Status = a.Status,
                    CategoryName = a.Category.Name
                })
                .ToListAsync();
        }

        public async Task<int> GetTotalAssetCountAsync()
            => await _assets.AllAsNoTracking().CountAsync();

        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            var hasAny = await _assets.AllAsNoTracking().AnyAsync();
            if (!hasAny)
            {
                return 0m;
            }

            return await _assets.AllAsNoTracking().SumAsync(a => a.Value);
        }

        public async Task<bool> ExistsAsync(int id)
            => await _assets.AllAsNoTracking().AnyAsync(a => a.Id == id);

        // ---- private business validation (server-side, defence in depth) ----
        private async Task ValidateAsync(AssetFormModel model, bool isUpdate)
        {
            if (model.Value < 0)
            {
                throw new ServiceValidationException("Asset value cannot be negative.");
            }

            if (model.Value > MaxAssetValue)
            {
                throw new ServiceValidationException(
                    $"Asset value cannot exceed {MaxAssetValue:C}.");
            }

            if (string.IsNullOrWhiteSpace(model.SerialNumber))
            {
                throw new ServiceValidationException("Serial number is required.");
            }

            var serial = model.SerialNumber.Trim();

            // Serial number must remain unique across the inventory.
            var duplicate = await _assets
                .AllAsNoTracking()
                .AnyAsync(a => a.SerialNumber == serial && (!isUpdate || a.Id != model.Id));

            if (duplicate)
            {
                throw new ServiceValidationException(
                    $"An asset with serial number '{serial}' already exists.");
            }
        }
    }
}
