using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ProductManagementApp.Data;
using ProductManagementApp.Models;
using System.Security.Claims;
using System;
using System.Linq;

namespace ProductManagementApp.Controllers
{
     [Authorize]
    public class ProductCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ProductCategory
        public IActionResult Index()
        {
            var categories = _context.ProductCategories
                .Where(c => c.DeletedAt == null)
                .Include(c => c.User)
                .ToList();

            return View(categories);
        }

        // GET: /ProductCategory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /ProductCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductCategory category)
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
            {
                return Content("User claim not found. User is not logged in or claim missing.");
            }
            var userIdStr = userIdClaim.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Content($"UserId claim invalid: {userIdStr}");
            }
            
            category.UserId = userId;
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            _context.ProductCategories.Add(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: /ProductCategory/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = _context.ProductCategories.Find(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: /ProductCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ProductCategory category)
        {
            if (id != category.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(category);

            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
            {
                return Content("User claim not found. User is not logged in or claim missing.");
            }
            var userIdStr = userIdClaim.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Content($"UserId claim invalid: {userIdStr}");
            }

            // Pastikan userId tetap benar
            category.UserId = userId;
            category.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.Update(category);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ProductCategories.Any(e => e.Id == category.Id))
                    return NotFound();
                else
                    throw;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. Try again.");
                return View(category);
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: /ProductCategory/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = _context.ProductCategories.Find(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: /ProductCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.ProductCategories.Find(id);
            if (category != null)
            {
                category.DeletedAt = DateTime.UtcNow; // soft delete
                _context.Update(category);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
