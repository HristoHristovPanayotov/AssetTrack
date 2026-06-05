using AssetTrack.Data.Seeding;
using AssetTrack.Services.Contracts;
using AssetTrack.Services.Exceptions;
using AssetTrack.Services.Models.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetTrack.Web.Controllers
{
    [Authorize(Roles = SeedConstants.AdministratorRoleName)]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: /Categories
        public async Task<IActionResult> Index()
        {
            var model = await _categoryService.GetAllAsync();
            return View(model);
        }

        // GET: /Categories/Create
        public IActionResult Create() => View(new CategoryFormModel());

        // POST: /Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _categoryService.CreateAsync(model);
            }
            catch (ServiceValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Category '{model.Name}' created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _categoryService.GetForEditAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: /Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _categoryService.UpdateAsync(model);
            }
            catch (ServiceValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Category '{model.Name}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
