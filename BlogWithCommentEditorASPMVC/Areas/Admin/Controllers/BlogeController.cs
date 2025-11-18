using BlogWithCommentEditorASPMVC.Areas.Admin.Models.Dtos.Blog;
using BlogWithCommentEditorASPMVC.Data;
using BlogWithCommentEditorASPMVC.Models.Entities.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace BlogWithCommentEditorASPMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBlogDto dto, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Please correct the errors in the form.");
                return View(dto);
            }
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "Unable to determine the current user.");
                    return View(dto);
                }

                BlogPst newPost = new()
                {
                    Id = Guid.CreateVersion7(),
                    Title = dto.Title,
                    Content = dto.Content,
                    AppUserId = Guid.Parse(userId!),
                    CreatedAt = DateTime.UtcNow,
                    ImageThumbnail = SaveImage(file)
                };


                await _context.BlogPosts.AddAsync(newPost);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");

            }
            catch (DbUpdateException dbEx)
            {
                ModelState.AddModelError(string.Empty, "A database error occurred while saving the blog post. Please try again.");
                return View(dto);
            }
            catch (IOException ioEx)
            {
                ModelState.AddModelError(string.Empty, "There was a problem saving the image. Please try again with a different file.");
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                return View(dto);
            }
        }
        // Helper function
        private string SaveImage(IFormFile file, string existingPath = null)
        {
            if (file == null || file.Length == 0)
                return existingPath; // nothing uploaded, keep old path

            // ✅ Enforce max size (5 MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5 MB in bytes
            if (file.Length > maxFileSize)
                throw new InvalidOperationException("File size cannot exceed 5 MB.");

            // ✅ Validate file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed.");

            // ✅ Validate MIME type
            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid file type. Only image files are allowed.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Delete old image if provided
            if (!string.IsNullOrEmpty(existingPath))
            {
                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingPath.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Generate unique filename
            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Return relative path for DB
            return "/images/" + uniqueFileName;
        }

      
    }
    
}
