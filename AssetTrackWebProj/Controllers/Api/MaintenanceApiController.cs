using AssetTrack.Services.Contracts;
using AssetTrack.Services.Models.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetTrack.Web.Controllers.Api
{
    /// <summary>
    /// JSON Web API consumed by the AJAX fetch() block on the Asset Details page.
    /// </summary>
    [ApiController]
    [Route("api/maintenance")]
    [Authorize]
    public class MaintenanceApiController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public MaintenanceApiController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // GET: /api/maintenance/{assetId}
        [HttpGet("{assetId:int}")]
        [ProducesResponseType(typeof(IEnumerable<TicketViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TicketViewModel>>> GetForAsset(int assetId)
        {
            if (assetId <= 0)
            {
                return BadRequest(new { message = "A valid asset id is required." });
            }

            var tickets = await _ticketService.GetTicketsForAssetAsync(assetId);
            return Ok(tickets);
        }
    }
}
