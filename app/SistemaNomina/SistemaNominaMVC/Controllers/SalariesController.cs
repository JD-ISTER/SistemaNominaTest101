using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaNominaMVC.Attributes;
using SistemaNominaMVC.Data;
using SistemaNominaMVC.Models;
using SistemaNominaMVC.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaNominaMVC.Controllers
{
    [SessionAuthorize("Administrador", "RRHH")]
    public class SalariesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SalariesController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Salaries
        public async Task<IActionResult> Index(string searchString, int? employeeId, string estado)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["EmployeeId"] = employeeId;
            ViewData["Estado"] = estado;

            var salaries = _context.Salaries
                .Include(s => s.Employee)
                .Where(s => s.Employee != null && s.Employee.IsActive)
                .AsQueryable();

            // Filtrar por empleado específico
            if (employeeId.HasValue)
            {
                salaries = salaries.Where(s => s.emp_no == employeeId);
            }

            // Filtrar por texto de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                salaries = salaries.Where(s =>
                    s.Employee != null && (
                        s.Employee.first_name.Contains(searchString) ||
                        s.Employee.last_name.Contains(searchString) ||
                        s.Employee.ci.Contains(searchString) ||
                        s.Employee.correo.Contains(searchString)));
            }

            // Filtrar por estado
            if (!string.IsNullOrEmpty(estado))
            {
                if (estado == "activos")
                {
                    salaries = salaries.Where(s => s.to_date == null);
                }
                else if (estado == "historicos")
                {
                    salaries = salaries.Where(s => s.to_date != null);
                }
            }

            // Cargar lista de empleados para el filtro
            var empleados = await _context.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.last_name)
                .Select(e => new
                {
                    Id = e.emp_no,
                    NombreCompleto = e.first_name + " " + e.last_name + " - " + e.ci
                })
                .ToListAsync();

            ViewBag.Employees = new SelectList(empleados, "Id", "NombreCompleto", employeeId);

            return View(await salaries
                .OrderByDescending(s => s.from_date)
                .ThenBy(s => s.Employee.last_name)
                .ToListAsync());
        }

        // GET: Salaries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salary = await _context.Salaries
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (salary == null)
            {
                return NotFound();
            }

            return View(salary);
        }

        // GET: Salaries/Create
        public async Task<IActionResult> Create(int? employeeId)
        {
            var viewModel = new SalaryCreateViewModel();

            if (employeeId.HasValue)
            {
                viewModel.EmployeeId = employeeId;
                viewModel.emp_no = employeeId.Value;

                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee != null)
                {
                    viewModel.EmployeeName = $"{employee.first_name} {employee.last_name}";
                }

                var salarioActual = await _context.Salaries
                    .FirstOrDefaultAsync(s => s.emp_no == employeeId && s.to_date == null);

                if (salarioActual != null)
                {
                    viewModel.CurrentSalary = salarioActual.salary;
                    viewModel.CurrentSalaryFromDate = salarioActual.from_date;
                }
            }

            // Cargar lista de empleados para el dropdown
            var empleados = await _context.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.last_name)
                .Select(e => new
                {
                    Id = e.emp_no,
                    NombreCompleto = e.first_name + " " + e.last_name + " - " + e.ci
                })
                .ToListAsync();

            ViewBag.Employees = new SelectList(empleados, "Id", "NombreCompleto", employeeId);

            return View(viewModel);
        }

        // POST: Salaries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalaryCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Verificar que el empleado existe
                    var employee = await _context.Employees
                        .FirstOrDefaultAsync(e => e.emp_no == viewModel.emp_no && e.IsActive);

                    if (employee == null)
                    {
                        ModelState.AddModelError("emp_no", "El empleado no existe o está inactivo");

                        // Recargar lista de empleados
                        var empleados = await _context.Employees
                            .Where(e => e.IsActive)
                            .OrderBy(e => e.last_name)
                            .Select(e => new
                            {
                                Id = e.emp_no,
                                NombreCompleto = e.first_name + " " + e.last_name + " - " + e.ci
                            })
                            .ToListAsync();

                        ViewBag.Employees = new SelectList(empleados, "Id", "NombreCompleto", viewModel.emp_no);
                        return View(viewModel);
                    }

                    // Verificar que no exista ya un salario activo en la misma fecha
                    var salarioExistente = await _context.Salaries
                        .FirstOrDefaultAsync(s => s.emp_no == viewModel.emp_no &&
                                                 s.from_date == viewModel.from_date);

                    if (salarioExistente != null)
                    {
                        ModelState.AddModelError("from_date", "Ya existe un salario registrado con esta fecha de inicio");

                        // Recargar lista de empleados
                        var empleados = await _context.Employees
                            .Where(e => e.IsActive)
                            .OrderBy(e => e.last_name)
                            .Select(e => new
                            {
                                Id = e.emp_no,
                                NombreCompleto = e.first_name + " " + e.last_name + " - " + e.ci
                            })
                            .ToListAsync();

                        ViewBag.Employees = new SelectList(empleados, "Id", "NombreCompleto", viewModel.emp_no);
                        return View(viewModel);
                    }

                    // Cerrar salario anterior si existe
                    var salarioAnterior = await _context.Salaries
                        .FirstOrDefaultAsync(s => s.emp_no == viewModel.emp_no && s.to_date == null);

                    if (salarioAnterior != null)
                    {
                        // Validar que la nueva fecha no sea anterior a la fecha de inicio del salario anterior
                        if (viewModel.from_date <= salarioAnterior.from_date)
                        {
                            ModelState.AddModelError("from_date", "La fecha de inicio debe ser posterior a la fecha de inicio del salario anterior");

                            // Recargar lista de empleados
                            var empleados = await _context.Employees
                                .Where(e => e.IsActive)
                                .OrderBy(e => e.last_name)
                                .Select(e => new
                                {
                                    Id = e.emp_no,
                                    NombreCompleto = e.first_name + " " + e.last_name + " - " + e.ci
                                })
                                .ToListAsync();

                            ViewBag.Employees = new SelectList(empleados, "Id", "NombreCompleto", viewModel.emp_no);
                            return View(viewModel);
                        }

                        salarioAnterior.to_date = viewModel.from_date.AddDays(-1);
                        _context.Update(salarioAnterior);
                    }

                    // Crear nuevo salario
                    var salary = new Salary
                    {
                        emp_no = viewModel.emp_no,
                        salary = viewModel.salary,
                        from_date = viewModel.from_date,
                        to_date = null
                    };

                    _context.Add(salary);
                    await _context.SaveChangesAsync();

                    // Registrar en auditoría
                    var userName = _httpContextAccessor.HttpContext?.Session.GetString("UserName") ?? "Sistema";
                    var log = new LogAuditoriaSalario
                    {
                        emp_no = viewModel.emp_no,
                        usuario = userName,
                        fechaActualizacion = DateTime.Now,
                        DetalleCambio = salarioAnterior == null
                            ? $"Creación de salario inicial: {viewModel.salary:C}"
                            : $"Cambio de salario de {salarioAnterior.salary:C} a {viewModel.salary:C}",
                        salario = viewModel.salary
                    };
                    _context.LogAuditoriaSalarios.Add(log);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["Success"] = "Salario registrado exitosamente";
                    return RedirectToAction(nameof(Index), new { employeeId = viewModel.emp_no });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Error al registrar el salario: " + ex.Message);
                }
            }

            // Si llegamos aquí, algo salió mal - recargar la lista de empleados
            var empleadosList = await _context.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.last_name)
                .Select(e => new
                {
                    Id = e.emp_no,
                    NombreCompleto = e.first_name + " " + e.last_name + " - " + e.ci
                })
                .ToListAsync();

            ViewBag.Employees = new SelectList(empleadosList, "Id", "NombreCompleto", viewModel.emp_no);

            return View(viewModel);
        }

        // POST: Salaries/Close/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var salary = await _context.Salaries
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salary == null)
            {
                return NotFound();
            }

            try
            {
                salary.to_date = DateTime.Today;
                _context.Update(salary);
                await _context.SaveChangesAsync();

                // Registrar en auditoría
                var userName = _httpContextAccessor.HttpContext?.Session.GetString("UserName") ?? "Sistema";
                var log = new LogAuditoriaSalario
                {
                    emp_no = salary.emp_no,
                    usuario = userName,
                    fechaActualizacion = DateTime.Now,
                    DetalleCambio = $"Cierre de salario: {salary.salary:C} (vigente hasta {DateTime.Today:dd/MM/yyyy})",
                    salario = salary.salary
                };
                _context.LogAuditoriaSalarios.Add(log);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Salario de {salary.Employee?.first_name} {salary.Employee?.last_name} cerrado exitosamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cerrar el salario: " + ex.Message;
            }

            return RedirectToAction(nameof(Index), new { employeeId = salary.emp_no });
        }

        // GET: Salaries/History/5
        public async Task<IActionResult> History(int employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Salaries)
                .FirstOrDefaultAsync(e => e.emp_no == employeeId && e.IsActive);

            if (employee == null)
            {
                return NotFound();
            }

            // Ordenar salarios por fecha descendente
            employee.Salaries = employee.Salaries
                .OrderByDescending(s => s.from_date)
                .ToList();

            return View(employee);
        }

        // GET: Salaries/Statistics
        public async Task<IActionResult> Statistics()
        {
            var statistics = new
            {
                TotalSalarios = await _context.Salaries.CountAsync(),
                SalariosActivos = await _context.Salaries.CountAsync(s => s.to_date == null),
                SalarioPromedio = await _context.Salaries
                    .Where(s => s.to_date == null)
                    .AverageAsync(s => (double?)s.salary) ?? 0,
                SalarioMaximo = await _context.Salaries
                    .Where(s => s.to_date == null)
                    .MaxAsync(s => (decimal?)s.salary) ?? 0,
                SalarioMinimo = await _context.Salaries
                    .Where(s => s.to_date == null)
                    .MinAsync(s => (decimal?)s.salary) ?? 0,
                EmpleadosSinSalario = await _context.Employees
                    .CountAsync(e => e.IsActive && !_context.Salaries.Any(s => s.emp_no == e.emp_no && s.to_date == null))
            };

            return Json(statistics);
        }

        // GET: Salaries/GetSalariesByEmployee
        public async Task<IActionResult> GetSalariesByEmployee(int employeeId)
        {
            var salaries = await _context.Salaries
                .Where(s => s.emp_no == employeeId)
                .OrderByDescending(s => s.from_date)
                .Select(s => new
                {
                    s.Id,
                    s.salary,
                    from_date = s.from_date.ToString("dd/MM/yyyy"),
                    to_date = s.to_date.HasValue ? s.to_date.Value.ToString("dd/MM/yyyy") : "Actual",
                    isActive = s.to_date == null
                })
                .ToListAsync();

            return Json(salaries);
        }

        // GET: Salaries/Export
        public async Task<IActionResult> Export(int? employeeId, string format = "excel")
        {
            var salaries = _context.Salaries
                .Include(s => s.Employee)
                .Where(s => s.Employee != null && s.Employee.IsActive)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                salaries = salaries.Where(s => s.emp_no == employeeId);
            }

            var data = await salaries
                .OrderByDescending(s => s.from_date)
                .ToListAsync();

            if (format.ToLower() == "excel")
            {
                // Aquí iría la lógica para exportar a Excel
                // Por ahora redirigimos al índice
                TempData["Info"] = "Funcionalidad de exportación en desarrollo";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Aquí iría la lógica para exportar a PDF
                TempData["Info"] = "Funcionalidad de exportación en desarrollo";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Salaries/VerifyOverlap
        [HttpGet]
        public async Task<IActionResult> VerifyOverlap(int employeeId, DateTime fromDate, int? excludeId = null)
        {
            var query = _context.Salaries
                .Where(s => s.emp_no == employeeId);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            // Verificar si hay algún salario que se solape con las fechas
            var overlapping = await query
                .AnyAsync(s => (s.from_date <= fromDate && (s.to_date == null || s.to_date >= fromDate)));

            return Json(new { hasOverlap = overlapping });
        }

        private bool SalaryExists(int id)
        {
            return _context.Salaries.Any(e => e.Id == id);
        }
    }
}