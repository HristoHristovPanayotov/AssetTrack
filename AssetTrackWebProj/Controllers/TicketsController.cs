using AssetTrack.Data.Models;
using AssetTrack.Data.Seeding;
using AssetTrack.Services.Contracts;
using AssetTrack.Services.Exceptions;
using AssetTrack.Services.Models.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AssetTrack.Web.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly IAssetService _assetService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsController(
            ITicketService ticketService,
            IAssetService assetService,
            UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _assetService = assetService;
            _userManager = userManager;
        }

        // GET: /Tickets/Create  -> employee reports a malfunction.
        public async Task<IActionResult> Create()
        {
            var model = new TicketInputModel
            {
                AvailableAssets = await GetReportableAssetsAsync()
            };
            return View(model);
        }

        // POST: /Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketInputModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableAssets = await GetReportableAssetsAsync();
                return View(model);
            }

            try
            {
                await _ticketService.CreateAsync(model);
            }
            catch (ServiceValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AvailableAssets = await GetReportableAssetsAsync();
                return View(model);
            }

            TempData["SuccessMessage"] =
                "Maintenance ticket submitted. IT has been notified.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Tickets/Open  -> admin queue of open tickets.
        [Authorize(Roles = SeedConstants.AdministratorRoleName)]
        public async Task<IActionResult> Open()
        {
            var tickets = await _ticketService.GetOpenTicketsAsync();
            return View(tickets);
        }

        // POST: /Tickets/Resolve  -> admin closes a ticket.
        [HttpPost]
        [Authorize(Roles = SeedConstants.AdministratorRoleName)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(int id, decimal repairCost)
        {
            try
            {
                await _ticketService.ResolveAsync(id, repairCost);
                TempData["SuccessMessage"] = "Ticket resolved and asset returned to service.";
            }
            catch (ServiceValidationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Open));
        }

        private async Task<IEnumerable<Services.Models.Assets.AssetListItemViewModel>>
            GetReportableAssetsAsync()
        {
            // Admins can file against any asset; employees only against their own gear.
            if (User.IsInRole(SeedConstants.AdministratorRoleName))
            {
                return await _assetService.GetAllSimpleAsync();
            }

            var userId = _userManager.GetUserId(User)!;
            return await _assetService.GetAssetsForUserAsync(userId);
        }
    }
}
