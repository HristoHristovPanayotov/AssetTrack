using System.Security.Claims;
using AssetTrack.Data.Models;
using AssetTrack.Data.Seeding;
using AssetTrack.Services.Contracts;
using AssetTrack.Services.Exceptions;
using AssetTrack.Services.Models.Assets;
using AssetTrack.Services.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssetTrack.Web.Controllers
{
    [Authorize]
    public class AssetsController : Controller
    {
        private const int PageSize = 5;

        private readonly IAssetService _assetService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssetsController(
            IAssetService assetService,
            ICategoryService categoryService,
            UserManager<ApplicationUser> userManager)
        {
            _assetService = assetService;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        // GET: /Assets  -> paginated, sortable, searchable master grid.
        public async Task<IActionResult> Index(string? search, string? sortOrder, int page = 1)
        {
            var model = await _assetService.GetAssetsAsync(search, sortOrder, page, PageSize);
            return View(model);
        }

        // GET: /Assets/MyGear -> assets assigned to the current employee.
        public async Task<IActionResult> MyGear()
        {
            var userId = _userManager.GetUserId(User)!;
            var assets = await _assetService.GetAssetsForUserAsync(userId);
            return View(assets);
        }

        // GET: /Assets/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var model = await _assetService.GetDetailsAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: /Assets/Create  (admin only)
        [Authorize(Roles = SeedConstants.AdministratorRoleName)]
        public async Task<IActionResult> Create()
        {
            var model = new AssetFormModel { PurchaseDate = DateTime.Today };
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        // POST: /Assets/Create  (admin only)
        [HttpPost]
        [Authorize(Roles = SeedConstants.AdministratorRoleName)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetFormModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            try
            {
                await _assetService.CreateAsync(model);
            }
            catch (ServiceValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Asset '{model.Name}' registered successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Assets/Edit/5  (admin only)
        [Authorize(Roles = SeedConstants.AdministratorRoleName)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _assetService.GetForEditAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync(model);
            return View(model);
        }

        // POST: /Assets/Edit/5  (admin only)
        [HttpPost]
        [Authorize(Roles = SeedConstants.AdministratorRoleName)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AssetFormModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            try
            {
                await _assetService.UpdateAsync(model);
            }
            catch (ServiceValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Asset '{model.Name}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Assets/Delete/5  (admin only) - confirmation screen.
        [Authorize(Roles = SeedConstants.AdministratorRoleName)]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _assetService.GetDetailsAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: /Assets/Delete/5  (admin only)
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = SeedConstants.AdministratorRoleName)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _assetService.DeleteAsync(id);
            }
            catch (ServiceValidationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Asset removed from active tracking.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync(AssetFormModel model)
        {
            model.Categories = await _categoryService.GetCategoryOptionsAsync();
            model.Users = _userManager.Users
                .OrderBy(u => u.FirstName)
                .Select(u => new UserOption
                {
                    Id = u.Id,
                    DisplayName = u.FirstName + " " + u.LastName + " (" + u.Department + ")"
                })
                .ToList();
        }
    }
}
