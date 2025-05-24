using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagementApp.Data;
using ProductManagementApp.Models;
using Microsoft.AspNetCore.Authorization; 
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductCategory category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            // Ambil user id dari claim (pastikan user sudah login)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);
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
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    category.UpdatedAt = DateTime.UtcNow;
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
                return RedirectToAction(nameof(Index));
            }
            return View(category);
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
