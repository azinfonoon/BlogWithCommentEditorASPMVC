using BlogWithCommentEditorASPMVC.Data;
using BlogWithCommentEditorASPMVC.Models.Dtos.User;
using BlogWithCommentEditorASPMVC.Models.Entities.User;
using Humanizer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        [HttpPost]
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
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                AppUser? user = await _Context.AppUsers.FirstOrDefaultAsync(u => u.UserName == dto.UserName);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                    return View(dto);
                }


                var result = _PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
                if (result == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                    return View(dto);
                }

                if (result == PasswordVerificationResult.Success)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = dto.RememberMe,
                        ExpiresUtc = dto.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(1)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return Unauthorized("Invalid login attempt.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                ModelState.AddModelError(string.Empty, "A database error occurred. Please try again later.");
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                return View(dto);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");


        }
    }
}