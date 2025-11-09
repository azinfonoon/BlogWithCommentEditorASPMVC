using BlogWithCommentEditorASPMVC.Data;
using BlogWithCommentEditorASPMVC.Models.Dtos.User;
using BlogWithCommentEditorASPMVC.Models.Entities.User;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogWithCommentEditorASPMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _Context;
        private readonly PasswordHasher<AppUser> _PasswordHasher;

        public AccountController(ApplicationDbContext context, PasswordHasher<AppUser> passwordHasher)
        {
            _Context = context;
            _PasswordHasher = passwordHasher;
        }
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            return View();
        }
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                var isUserExit = await _Context.AppUsers.AnyAsync(u => u.UserName == dto.UserName || u.Email == dto.Email);
                if (isUserExit)
                {
                    ModelState.AddModelError(string.Empty, "User with the same username or email already exists.");
                    return View(dto);
                }


                AppUser newuser = new()
                {
                    Id = Guid.CreateVersion7(),
                    UserName = dto.UserName,
                    Email = dto.Email,
                    Role = "user",
                    RegisteredAt = DateTime.UtcNow,
                };


                newuser.PasswordHash = _PasswordHasher.HashPassword(newuser, dto.Password);

                await _Context.AppUsers.AddAsync(newuser);
                await _Context.SaveChangesAsync();
                return RedirectToAction("Login");
            }

            catch (DbUpdateException db)
            {
                ModelState.AddModelError(string.Empty, "A database error occurred while registering. Please try again.");
                return View(dto);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                return View(dto);
            }
        }
    }

} 
 