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
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Departments
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var departments = from d in _context.Departments
                              where d.IsActive
                              select d;

            if (!string.IsNullOrEmpty(searchString))
            {
                departments = departments.Where(d =>
                    d.dept_name.Contains(searchString) ||
                    d.dept_no.Contains(searchString));
            }

            return View(await departments.OrderBy(d => d.dept_no).ToListAsync());
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.DeptEmps)
                    .ThenInclude(de => de.Employee)
                .Include(d => d.DeptManagers)
                    .ThenInclude(dm => dm.Employee)
                .FirstOrDefaultAsync(m => m.dept_no == id && m.IsActive);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("dept_no,dept_name")] Department department)
        {
            if (ModelState.IsValid)
            {
                // Verificar si ya existe el código
                if (await _context.Departments.AnyAsync(d => d.dept_no == department.dept_no))
                {
                    ModelState.AddModelError("dept_no", "Ya existe un departamento con este código");
                    return View(department);
                }

                // Verificar si ya existe el nombre
                if (await _context.Departments.AnyAsync(d => d.dept_name == department.dept_name))
                {
                    ModelState.AddModelError("dept_name", "Ya existe un departamento con este nombre");
                    return View(department);
                }

                department.IsActive = true;
                _context.Add(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Departamento creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null || !department.IsActive)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("dept_no,dept_name,IsActive")] Department department)
        {
            if (id != department.dept_no)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar nombre único (excluyendo el actual)
                    if (await _context.Departments.AnyAsync(d => d.dept_name == department.dept_name && d.dept_no != id))
                    {
                        ModelState.AddModelError("dept_name", "Ya existe un departamento con este nombre");
                        return View(department);
                    }

                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Departamento actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.dept_no))
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
            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            // Verificar si tiene empleados asignados
            var tieneEmpleados = await _context.DeptEmps.AnyAsync(de => de.dept_no == id && de.to_date == null);
            if (tieneEmpleados)
            {
                TempData["Error"] = "No se puede desactivar el departamento porque tiene empleados asignados";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                department.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Departamento desactivado exitosamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al desactivar el departamento: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(string id)
        {
            return _context.Departments.Any(e => e.dept_no == id && e.IsActive);
        }

        // GET: Departments/VerifyCode
        [HttpGet]
        public async Task<IActionResult> VerifyCode(string code)
        {
            var exists = await _context.Departments.AnyAsync(d => d.dept_no == code);
            return Json(new { exists });
        }

        // GET: Departments/VerifyName
        [HttpGet]
        public async Task<IActionResult> VerifyName(string name)
        {
            var exists = await _context.Departments.AnyAsync(d => d.dept_name == name);
            return Json(new { exists });
        }
    }
}