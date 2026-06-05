using AssetTrack.Services.Models.Tickets;

namespace AssetTrack.Services.Contracts
{
    public interface ITicketService
    {
        /// <summary>
        /// Files a repair ticket and flips the target asset to UnderMaintenance.
        /// </summary>
        Task<int> CreateAsync(TicketInputModel model);

        Task ResolveAsync(int ticketId, decimal repairCost);

        Task<IEnumerable<TicketViewModel>> GetTicketsForAssetAsync(int assetId);

        Task<IEnumerable<TicketViewModel>> GetOpenTicketsAsync();

        Task<int> GetOpenTicketCountAsync();

        Task<decimal> GetTotalRepairExpenditureAsync();
    }
}
