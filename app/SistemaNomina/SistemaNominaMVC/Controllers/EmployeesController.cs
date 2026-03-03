using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaNominaMVC.Attributes;
using SistemaNominaMVC.Data;
using SistemaNominaMVC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaNominaMVC.Controllers
{
    [SessionAuthorize("Administrador", "RRHH")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmployeesController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;

            var employees = from e in _context.Employees
                            where e.IsActive
                            select e;

            if (!string.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e =>
                    e.first_name.Contains(searchString) ||
                    e.last_name.Contains(searchString) ||
                    e.ci.Contains(searchString) ||
                    e.correo.Contains(searchString));
            }

            // Ordenar por defecto
            employees = employees.OrderBy(e => e.last_name).ThenBy(e => e.first_name);

            int pageSize = 10;
            return View(await PaginatedList<Employee>.CreateAsync(employees.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.DeptEmps)
                    .ThenInclude(d => d.Department)
                .Include(e => e.Titles)
                .Include(e => e.Salaries)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.emp_no == id && m.IsActive);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ci,birth_date,first_name,last_name,gender,hire_date,correo")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar CI único
                    if (await _context.Employees.AnyAsync(e => e.ci == employee.ci))
                    {
                        ModelState.AddModelError("ci", "Ya existe un empleado con este CI");
                        return View(employee);
                    }

                    // Verificar correo único
                    if (await _context.Employees.AnyAsync(e => e.correo == employee.correo))
                    {
                        ModelState.AddModelError("correo", "Ya existe un empleado con este correo");
                        return View(employee);
                    }

                    employee.IsActive = true;
                    _context.Add(employee);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Empleado creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el empleado: " + ex.Message);
                }
            }
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null || !employee.IsActive)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("emp_no,ci,birth_date,first_name,last_name,gender,hire_date,correo")] Employee employee)
        {
            if (id != employee.emp_no)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar CI único (excluyendo el actual)
                    if (await _context.Employees.AnyAsync(e => e.ci == employee.ci && e.emp_no != id))
                    {
                        ModelState.AddModelError("ci", "Ya existe un empleado con este CI");
                        return View(employee);
                    }

                    // Verificar correo único (excluyendo el actual)
                    if (await _context.Employees.AnyAsync(e => e.correo == employee.correo && e.emp_no != id))
                    {
                        ModelState.AddModelError("correo", "Ya existe un empleado con este correo");
                        return View(employee);
                    }

                    employee.IsActive = true;
                    _context.Update(employee);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Empleado actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.emp_no))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            try
            {
                // Soft delete
                employee.IsActive = false;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Empleado desactivado exitosamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al desactivar el empleado: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.emp_no == id && e.IsActive);
        }
    }
}