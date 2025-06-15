using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductManagementApp.Data;
using ProductManagementApp.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProductManagementApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var product = await _context.Products
                                .Where(p => p.DeletedAt == null)  
                                .Include(p => p.Category)
                                .ToListAsync();
            return View(product);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                                .Include(p => p.Category)
                                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            // Ambil semua kategori yang belum dihapus (DeletedAt == null)
            var categories = _context.ProductCategories
                                    .Where(c => c.DeletedAt == null)
                                    .ToList();

            // Buat SelectList dengan value = Id dan text = Name
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");

            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? ImageFile)
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
            {
                return Content("User claim not found. User is not logged in or claim missing.");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Content("User ID claim is invalid.");
            }

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    model.ImageUrl = "/uploads/" + uniqueFileName;
                }

                model.UserId = userId;

                _context.Products.Add(model);
                await _context.SaveChangesAsync();

                // ✅ TempData diletakkan di sini agar bisa muncul setelah redirect
                TempData["SuccessMessage"] = "Produk berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }

            // Jika ModelState tidak valid, tampilkan kembali form dengan kategori
            var categories = _context.ProductCategories
                                    .Where(c => c.DeletedAt == null)
                                    .ToList();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", model.CategoryId);

            return View(model);
        }




        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

        
            var categories = await _context.ProductCategories
                                        .Where(c => c.DeletedAt == null)
                                        .ToListAsync();

            
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        // POST: Products/Edit/5
       [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model, IFormFile? ImageFile)
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
            {
                return Content("User claim not found. User is not logged in or claim missing.");
            }
            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Content("User ID claim is invalid.");
            }

            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Update field yang boleh diubah
                    existingProduct.Name = model.Name;
                    existingProduct.Description = model.Description;
                    existingProduct.Price = model.Price;
                    existingProduct.CategoryId = model.CategoryId;
                    existingProduct.UserId = userId;

                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        existingProduct.ImageUrl = "/uploads/" + uniqueFileName;
                    }

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();

                    
                    TempData["SuccessMessage"] = "Produk berhasil diperbarui.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == model.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Jika ModelState tidak valid, siapkan kembali kategori dan tampilkan form
            var categories = _context.ProductCategories
                                    .Where(c => c.DeletedAt == null)
                                    .ToList();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", model.CategoryId);

            return View(model);
        }




        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                                .Include(p => p.Category)
                                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null && product.DeletedAt == null)
            {
                product.DeletedAt = DateTime.UtcNow; 
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMessage"] = "Produk berhasil dihapus.";
            return RedirectToAction(nameof(Index));
        }
    }
}
