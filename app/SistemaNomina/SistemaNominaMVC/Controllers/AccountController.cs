using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaNominaMVC.Data;
using SistemaNominaMVC.Helpers;
using SistemaNominaMVC.Models;
using System.Diagnostics;

namespace SistemaNominaMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Si ya está logueado, redirigir al dashboard
            if (HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string usuario, string clave)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(clave))
                {
                    ViewBag.Error = "Usuario y clave son requeridos";
                    return View();
                }

                var user = await _context.Users
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.usuario == usuario && u.IsActive);

                if (user == null)
                {
                    ViewBag.Error = "Usuario o contraseña incorrectos";
                    return View();
                }

                if (PasswordHelper.VerifyPassword(clave, user.clave))
                {
                    // Guardar en sesión
                    HttpContext.Session.SetUser(user);

                    // Registrar acceso
                    Console.WriteLine($"Usuario {user.usuario} accedió al sistema");

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Usuario o contraseña incorrectos";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al iniciar sesión: " + ex.Message;
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Logout();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}