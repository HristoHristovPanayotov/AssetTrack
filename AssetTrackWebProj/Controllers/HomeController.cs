using System.Diagnostics;
using AssetTrack.Services.Contracts;
using AssetTrack.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AssetTrack.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly ITicketService _ticketService;

        public HomeController(IAssetService assetService, ITicketService ticketService)
        {
            _assetService = assetService;
            _ticketService = ticketService;
        }

        // GET: /  -> dashboard with summary metric cards.
        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TotalAssets = await _assetService.GetTotalAssetCountAsync(),
                OpenTickets = await _ticketService.GetOpenTicketCountAsync(),
                TotalInventoryValue = await _assetService.GetTotalInventoryValueAsync(),
                TotalRepairSpend = await _ticketService.GetTotalRepairExpenditureAsync()
            };

            return View(model);
        }

        // GET: /Home/Error/{code}  -> custom, styled status pages.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Home/Error/{code:int?}")]
        public IActionResult Error(int? code)
        {
            var statusCode = code ?? 500;

            var (title, message) = statusCode switch
            {
                400 => ("400 - Bad Request", "The request could not be understood by the server."),
                401 => ("401 - Unauthorized", "You need to sign in to view this resource."),
                403 => ("403 - Forbidden", "You do not have permission to access this resource."),
                404 => ("404 - Not Found", "The page or resource you requested does not exist."),
                _ => ("500 - Server Error", "An unexpected error occurred while processing your request.")
            };

            var model = new ErrorViewModel
            {
                StatusCode = statusCode,
                Title = title,
                Message = message,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            Response.StatusCode = statusCode;
            return View(model);
        }
    }
}
