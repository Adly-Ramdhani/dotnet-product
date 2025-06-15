using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagementApp.Models;
using ProductManagementApp.Data; 
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class UserController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: User
    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .Where(u => u.DeletedAt == null)
            .ToListAsync();
        return View(users);
    }

    // GET: User/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);

        if (user == null) return NotFound();

        return View(user);
    }


    // GET: User/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: User/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FullName,Email,Password,ProfilePicture")] User user)
    {
        if (ModelState.IsValid)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // GET: User/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var user = await _context.Users.FindAsync(id);
        if (user == null || user.DeletedAt != null) return NotFound();

        return View(user);
    }

 // POST: User/Edit/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user)
    {
        if (id != user.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null || existingUser.DeletedAt != null)
                return NotFound();

            // ✅ Cek apakah email sudah digunakan oleh user lain
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == user.Email && u.Id != id && u.DeletedAt == null);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email sudah digunakan oleh pengguna lain.");
                return View(user);
            }

            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;

            if (!string.IsNullOrEmpty(user.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }

            if (user.ProfilePictureFile != null)
            {
                var fileName = Path.GetFileName(user.ProfilePictureFile.FileName);
                var filePath = Path.Combine("wwwroot/uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await user.ProfilePictureFile.CopyToAsync(stream);
                }

                existingUser.ProfilePicture = "/uploads/" + fileName;
            }

            existingUser.UpdatedAt = DateTime.UtcNow;
            _context.Update(existingUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profil berhasil diperbarui!";
            return RedirectToAction("Details", new { id = existingUser.Id });
        }

        return View(user);
    }






    // GET: User/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);

        if (user == null) return NotFound();

        return View(user);
    }

    // POST: User/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.DeletedAt != null) return NotFound();

        user.DeletedAt = DateTime.UtcNow;
        _context.Update(user);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool UserExists(int id) =>
        _context.Users.Any(e => e.Id == id && e.DeletedAt == null);
}
