using AssetTrack.Data.Models;
using AssetTrack.Data.Models.Enums;
using AssetTrack.Services.Contracts;
using AssetTrack.Services.Exceptions;
using AssetTrack.Services.Models.Tickets;
using AssetTrack.Services.Repository;
using Microsoft.EntityFrameworkCore;

namespace AssetTrack.Services.Implementations
{
    public class TicketService : ITicketService
    {
        private readonly IRepository<MaintenanceTicket> _tickets;
        private readonly IRepository<Asset> _assets;

        public TicketService(
            IRepository<MaintenanceTicket> tickets,
            IRepository<Asset> assets)
        {
            _tickets = tickets;
            _assets = assets;
        }

        public async Task<int> CreateAsync(TicketInputModel model)
        {
            var asset = await _assets.GetByIdAsync(model.AssetId)
                ?? throw new ServiceValidationException("The selected asset does not exist.");

            if (string.IsNullOrWhiteSpace(model.Description))
            {
                throw new ServiceValidationException("A problem description is required.");
            }

            var ticket = new MaintenanceTicket
            {
                AssetId = model.AssetId,
                Description = model.Description.Trim(),
                DateReported = DateTime.UtcNow,
                IsResolved = false,
                RepairCost = 0m
            };

            await _tickets.AddAsync(ticket);

            // Filing a ticket flips the asset into maintenance unless it is retired.
            if (asset.Status != AssetStatus.Retired)
            {
                asset.Status = AssetStatus.UnderMaintenance;
                _assets.Update(asset);
            }

            await _tickets.SaveChangesAsync();
            return ticket.Id;
        }

        public async Task ResolveAsync(int ticketId, decimal repairCost)
        {
            if (repairCost < 0)
            {
                throw new ServiceValidationException("Repair cost cannot be negative.");
            }

            var ticket = await _tickets.GetByIdAsync(ticketId)
                ?? throw new ServiceValidationException("The ticket could not be found.");

            ticket.IsResolved = true;
            ticket.RepairCost = repairCost;
            _tickets.Update(ticket);

            // Return the asset to service once all of its tickets are closed.
            var asset = await _assets.GetByIdAsync(ticket.AssetId);
            if (asset != null && asset.Status == AssetStatus.UnderMaintenance)
            {
                var hasOtherOpen = await _tickets
                    .AllAsNoTracking()
                    .AnyAsync(t => t.AssetId == asset.Id && t.Id != ticketId && !t.IsResolved);

                if (!hasOtherOpen)
                {
                    asset.Status = asset.AssignedUserId == null
                        ? AssetStatus.Available
                        : AssetStatus.Assigned;
                    _assets.Update(asset);
                }
            }

            await _tickets.SaveChangesAsync();
        }

        public async Task<IEnumerable<TicketViewModel>> GetTicketsForAssetAsync(int assetId)
        {
            return await _tickets
                .AllAsNoTracking()
                .Where(t => t.AssetId == assetId)
                .OrderByDescending(t => t.DateReported)
                .Select(t => new TicketViewModel
                {
                    Id = t.Id,
                    Description = t.Description,
                    DateReported = t.DateReported,
                    IsResolved = t.IsResolved,
                    RepairCost = t.RepairCost,
                    AssetId = t.AssetId
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketViewModel>> GetOpenTicketsAsync()
        {
            return await _tickets
                .AllAsNoTracking()
                .Where(t => !t.IsResolved)
                .OrderBy(t => t.DateReported)
                .Select(t => new TicketViewModel
                {
                    Id = t.Id,
                    Description = t.Description,
                    DateReported = t.DateReported,
                    IsResolved = t.IsResolved,
                    RepairCost = t.RepairCost,
                    AssetId = t.AssetId
                })
                .ToListAsync();
        }

        public async Task<int> GetOpenTicketCountAsync()
            => await _tickets.AllAsNoTracking().CountAsync(t => !t.IsResolved);

        public async Task<decimal> GetTotalRepairExpenditureAsync()
        {
            var hasAny = await _tickets.AllAsNoTracking().AnyAsync();
            if (!hasAny)
            {
                return 0m;
            }

            return await _tickets.AllAsNoTracking().SumAsync(t => t.RepairCost);
        }
    }
}
