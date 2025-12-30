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

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var query = _context.BlogPosts.AsNoTracking();

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // ✅ Prevent requesting a page beyond the last one
                if (totalPages > 0 && page > totalPages)
                {
                    // Option 1: redirect to last valid page
                    return RedirectToAction(nameof(Index), new { page = totalPages, pageSize });

                    // Option 2: return NotFound
                    // return NotFound();
                }

                var blogPosts = await query
                    .OrderByDescending(bp => bp.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(bg => new BlogIndexDto
                    {
                        Id = bg.Id,
                        Title = bg.Title,
                        CreatedAt = bg.CreatedAt,
                        AuthorName = bg.AppUser.UserName,
                    })
                    .ToListAsync();

                var viewModel = new PageBlogPostIndexViewModel
                {
                    BlogPosts = blogPosts,
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception with ILogger
                Console.WriteLine($"[Error] {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading posts.";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var post = await _context.BlogPosts
                    .AsNoTracking()
                    .Where(bp => bp.Id == id)
                    .Select(bp => new BlogPostDetailsViewModel()
                    {
                        Title = bp.Title,
                        Content = bp.Content,
                        CreatedAt = bp.CreatedAt,
                        ImageThumbnail = bp.ImageThumbnail,
                        AuthorName = bp.AppUser.UserName,
                        CommentCount = bp.Comments.Count
                    }).FirstOrDefaultAsync();

                if (post == null) return NotFound();

                return View(post);
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var post = await _context.BlogPosts.FindAsync(id);
                if (post == null)
                {
                    TempData["ErrorMessage"] = "Could not found post";
                    return RedirectToAction("Index");
                }

                // Delete the image file if it exists
                if (!string.IsNullOrEmpty(post.ImageThumbnail))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImageThumbnail.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Delete images inside Content
                if (!string.IsNullOrEmpty(post.Content))
                {
                    var imagePaths = ExtractImagePathsFromHtml(post.Content);
                    foreach (var imgPath in imagePaths)
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imgPath.TrimStart('/'));
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                }

                _context.BlogPosts.Remove(post);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Post deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (IOException ioEx)
            {
                TempData["ErrorMessage"] = "There was a problem deleting associated images.";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException dbEx)
            {
                TempData["ErrorMessage"] = "Database error occurred while deleting the blog post.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return RedirectToAction("Index");
            }
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

        private List<string> ExtractImagePathsFromHtml(string htmlContent)
        {
            var imagePaths = new List<string>();
            var regex = new Regex("<img[^>]+src=\"([^\"]+)\"", RegexOptions.IgnoreCase);
            var matches = regex.Matches(htmlContent);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    var src = match.Groups[1].Value;
                    if (src.StartsWith("/images/")) // only delete local images
                    {
                        imagePaths.Add(src);
                    }
                }
            }

            return imagePaths;
        }
    }
    
}
