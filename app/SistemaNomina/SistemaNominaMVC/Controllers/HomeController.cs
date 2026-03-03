using Microsoft.AspNetCore.Mvc;
using SistemaNominaMVC.Attributes;
using SistemaNominaMVC.Data;
using Microsoft.EntityFrameworkCore;

namespace SistemaNominaMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [SessionAuthorize]
        public async Task<IActionResult> Index()
        {
            // Datos para el dashboard
            ViewBag.TotalEmpleados = await _context.Employees.CountAsync(e => e.IsActive);
            ViewBag.TotalDepartamentos = await _context.Departments.CountAsync(d => d.IsActive);
            ViewBag.SalariosVigentes = await _context.Salaries.CountAsync(s => s.to_date == null);

            // Últimos empleados
            var ultimosEmpleados = await _context.Employees
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.hire_date)
                .Take(5)
                .ToListAsync();

            return View(ultimosEmpleados);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}