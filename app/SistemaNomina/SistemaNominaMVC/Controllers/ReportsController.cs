using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaNominaMVC.Attributes;
using SistemaNominaMVC.Data;
using SistemaNominaMVC.Helpers;
using SistemaNominaMVC.Models;
using SistemaNominaMVC.Models.ViewModels;

namespace SistemaNominaMVC.Controllers
{
    [SessionAuthorize("Administrador", "RRHH")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportsController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Reports
        public IActionResult Index()
        {
            return View();
        }

        #region Nómina Vigente

        // GET: Reports/NominaVigente
        public async Task<IActionResult> NominaVigente(string departmentId, DateTime? fecha)
        {
            try
            {
                ViewBag.Departments = new SelectList(
                    await _context.Departments
                        .Where(d => d.IsActive)
                        .OrderBy(d => d.dept_name)
                        .Select(d => new { d.dept_no, d.dept_name })
                        .ToListAsync(),
                    "dept_no",
                    "dept_name",
                    departmentId
                );

                fecha = fecha ?? DateTime.Today;
                ViewBag.Fecha = fecha;

                var query = from de in _context.DeptEmps
                            join s in _context.Salaries on de.emp_no equals s.emp_no
                            join e in _context.Employees on de.emp_no equals e.emp_no
                            join d in _context.Departments on de.dept_no equals d.dept_no
                            where de.from_date <= fecha &&
                                  (de.to_date == null || de.to_date >= fecha) &&
                                  s.from_date <= fecha &&
                                  (s.to_date == null || s.to_date >= fecha) &&
                                  e.IsActive && d.IsActive
                            select new NominaVigenteViewModel
                            {
                                EmpleadoId = e.emp_no,
                                CIEmpleado = e.ci,
                                NombreEmpleado = e.first_name,
                                ApellidoEmpleado = e.last_name,
                                EmailEmpleado = e.correo,
                                FechaIngreso = e.hire_date,
                                Salario = s.salary,
                                FechaInicioSalario = s.from_date,
                                FechaFinSalario = s.to_date,
                                EsSalarioVigente = s.to_date == null,
                                Departamento = d.dept_name,
                                CodigoDepartamento = d.dept_no
                            };

                if (!string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(x => x.CodigoDepartamento == departmentId);
                }

                // Obtener títulos por separado (mejor performance)
                var result = await query.ToListAsync();

                foreach (var item in result)
                {
                    var titulo = await _context.Titles
                        .Where(t => t.emp_no == item.EmpleadoId &&
                                   t.from_date <= fecha &&
                                   (t.to_date == null || t.to_date >= fecha))
                        .Select(t => t.title)
                        .FirstOrDefaultAsync();

                    item.TituloEmpleado = titulo ?? "Sin cargo";
                }

                return View(result.OrderBy(x => x.Departamento).ThenBy(x => x.ApellidoEmpleado));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al generar el reporte de nómina vigente: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Cambios Salariales

        // GET: Reports/CambiosSalariales
        public async Task<IActionResult> CambiosSalariales(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                fechaInicio = fechaInicio ?? DateTime.Today.AddMonths(-1);
                fechaFin = fechaFin ?? DateTime.Today;

                ViewBag.FechaInicio = fechaInicio;
                ViewBag.FechaFin = fechaFin;

                var logs = await _context.LogAuditoriaSalarios
                    .Include(l => l.Employee)
                    .Where(l => l.fechaActualizacion.Date >= fechaInicio.Value.Date &&
                               l.fechaActualizacion.Date <= fechaFin.Value.Date)
                    .OrderByDescending(l => l.fechaActualizacion)
                    .ToListAsync();

                var viewModel = logs.Select(l => {
                    decimal? salarioAnterior = null;
                    decimal? incremento = null;
                    string tipoCambio = "Creación";

                    if (l.DetalleCambio.Contains("Cambio"))
                    {
                        tipoCambio = "Modificación";
                        try
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(l.DetalleCambio, @"de \$?([\d,\.]+) a \$?([\d,\.]+)");
                            if (match.Success)
                            {
                                salarioAnterior = decimal.Parse(match.Groups[1].Value.Replace(",", "").Replace("$", ""));
                                incremento = l.salario - salarioAnterior.Value;
                            }
                        }
                        catch { }
                    }

                    return new CambioSalarialViewModel
                    {
                        Id = l.Id,
                        FechaActualizacion = l.fechaActualizacion,
                        Usuario = l.usuario,
                        EmpleadoId = l.emp_no,
                        NombreEmpleado = l.Employee?.first_name ?? "",
                        ApellidoEmpleado = l.Employee?.last_name ?? "",
                        CIEmpleado = l.Employee?.ci ?? "",
                        DetalleCambio = l.DetalleCambio,
                        Salario = l.salario,
                        SalarioAnterior = salarioAnterior,
                        Incremento = incremento,
                        TipoCambio = tipoCambio
                    };
                }).ToList();

                // Calcular estadísticas
                ViewBag.TotalCambios = viewModel.Count;
                ViewBag.EmpleadosAfectados = viewModel.Select(l => l.EmpleadoId).Distinct().Count();
                ViewBag.IncrementoPromedio = viewModel.Where(l => l.Incremento.HasValue && l.Incremento > 0)
                                                      .Average(l => (double?)l.Incremento) ?? 0;
                ViewBag.MontoTotal = viewModel.Sum(l => l.Salario);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al generar el reporte de cambios salariales: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Estructura Organizacional

        // GET: Reports/EstructuraOrganizacional
        public async Task<IActionResult> EstructuraOrganizacional()
        {
            try
            {
                var estructura = await _context.Departments
                    .Where(d => d.IsActive)
                    .Select(d => new
                    {
                        Departamento = d,
                        Gerente = _context.DeptManagers
                            .Where(dm => dm.dept_no == d.dept_no && dm.to_date == null)
                            .Select(dm => dm.Employee)
                            .FirstOrDefault(),
                        Empleados = _context.DeptEmps
                            .Where(de => de.dept_no == d.dept_no && de.to_date == null)
                            .Select(de => de.Employee)
                            .ToList()
                    })
                    .ToListAsync();

                // Cargar títulos y salarios para cada empleado (para la vista)
                var resultado = new List<dynamic>();
                foreach (var item in estructura)
                {
                    var empleadosConDatos = new List<dynamic>();
                    foreach (var emp in item.Empleados)
                    {
                        var titulo = await _context.Titles
                            .FirstOrDefaultAsync(t => t.emp_no == emp.emp_no && t.to_date == null);

                        var salario = await _context.Salaries
                            .FirstOrDefaultAsync(s => s.emp_no == emp.emp_no && s.to_date == null);

                        empleadosConDatos.Add(new
                        {
                            emp.emp_no,
                            emp.ci,
                            emp.first_name,
                            emp.last_name,
                            emp.correo,
                            emp.hire_date,
                            Titulo = titulo?.title ?? "Sin cargo",
                            Salario = salario?.salary ?? 0
                        });
                    }

                    resultado.Add(new
                    {
                        item.Departamento,
                        Gerente = item.Gerente != null ? new
                        {
                            item.Gerente.emp_no,
                            item.Gerente.ci,
                            item.Gerente.first_name,
                            item.Gerente.last_name,
                            item.Gerente.correo
                        } : null,
                        Empleados = empleadosConDatos
                    });
                }

                await LogActivity("Consulta de Estructura Organizacional", null);

                return View(resultado);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al generar la estructura organizacional: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Auditoría

        // GET: Reports/Auditoria
        public async Task<IActionResult> Auditoria(DateTime? fechaInicio, DateTime? fechaFin, string usuario)
        {
            try
            {
                fechaInicio = fechaInicio ?? DateTime.Today.AddMonths(-3);
                fechaFin = fechaFin ?? DateTime.Today;

                ViewBag.FechaInicio = fechaInicio;
                ViewBag.FechaFin = fechaFin;
                ViewBag.Usuario = usuario;

                var query = _context.LogAuditoriaSalarios
                    .Include(l => l.Employee)
                    .AsQueryable();

                query = query.Where(l => l.fechaActualizacion.Date >= fechaInicio.Value.Date &&
                                        l.fechaActualizacion.Date <= fechaFin.Value.Date);

                if (!string.IsNullOrEmpty(usuario))
                {
                    query = query.Where(l => l.usuario.Contains(usuario));
                }

                var logs = await query
                    .OrderByDescending(l => l.fechaActualizacion)
                    .ToListAsync();

                // Lista de usuarios para filtro
                ViewBag.Usuarios = new SelectList(
                    await _context.LogAuditoriaSalarios
                        .Select(l => l.usuario)
                        .Distinct()
                        .OrderBy(u => u)
                        .ToListAsync()
                );

                return View(logs);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al generar el reporte de auditoría: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Estadísticas

        // GET: Reports/Estadisticas
        public async Task<IActionResult> Estadisticas()
        {
            try
            {
                var estadisticas = new
                {
                    // Empleados
                    TotalEmpleados = await _context.Employees.CountAsync(e => e.IsActive),
                    EmpleadosPorGenero = await _context.Employees
                        .Where(e => e.IsActive)
                        .GroupBy(e => e.gender)
                        .Select(g => new { Genero = g.Key, Cantidad = g.Count() })
                        .ToListAsync(),

                    // Departamentos
                    TotalDepartamentos = await _context.Departments.CountAsync(d => d.IsActive),
                    EmpleadosPorDepartamento = await _context.DeptEmps
                        .Where(de => de.to_date == null)
                        .GroupBy(de => de.dept_no)
                        .Select(g => new {
                            DeptNo = g.Key,
                            Cantidad = g.Count(),
                            DeptName = _context.Departments
                                .Where(d => d.dept_no == g.Key)
                                .Select(d => d.dept_name)
                                .FirstOrDefault()
                        })
                        .ToListAsync(),

                    // Salarios
                    SalarioPromedio = await _context.Salaries
                        .Where(s => s.to_date == null)
                        .AverageAsync(s => (double?)s.salary) ?? 0,
                    SalarioMaximo = await _context.Salaries
                        .Where(s => s.to_date == null)
                        .MaxAsync(s => (decimal?)s.salary) ?? 0,
                    SalarioMinimo = await _context.Salaries
                        .Where(s => s.to_date == null)
                        .MinAsync(s => (decimal?)s.salary) ?? 0,

                    // Títulos
                    TitulosMasComunes = await _context.Titles
                        .Where(t => t.to_date == null)
                        .GroupBy(t => t.title)
                        .Select(g => new { Titulo = g.Key, Cantidad = g.Count() })
                        .OrderByDescending(g => g.Cantidad)
                        .Take(5)
                        .ToListAsync()
                };

                return Json(estadisticas);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        #endregion

        #region Exportación a Excel

        // GET: Reports/ExportarExcel
        public async Task<IActionResult> ExportarExcel(string tipo, string departmentId, DateTime? fecha,
                                                        DateTime? fechaInicio, DateTime? fechaFin, string usuario)
        {
            try
            {
                byte[] fileBytes = null;
                string fileName = "";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                switch (tipo?.ToLower())
                {
                    case "nomina":
                        fileBytes = await ExportarNominaExcel(departmentId, fecha ?? DateTime.Today);
                        fileName = $"NominaVigente_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        break;

                    case "cambios":
                        fileBytes = await ExportarCambiosExcel(fechaInicio ?? DateTime.Today.AddMonths(-1),
                                                               fechaFin ?? DateTime.Today);
                        fileName = $"CambiosSalariales_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        break;

                    case "estructura":
                        fileBytes = await ExportarEstructuraExcel();
                        fileName = $"EstructuraOrganizacional_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        break;

                    case "auditoria":
                        fileBytes = await ExportarAuditoriaExcel(fechaInicio ?? DateTime.Today.AddMonths(-3),
                                                                 fechaFin ?? DateTime.Today, usuario);
                        fileName = $"Auditoria_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        break;

                    default:
                        TempData["Error"] = "Tipo de reporte no válido";
                        return RedirectToAction(nameof(Index));
                }

                if (fileBytes != null)
                {
                    return File(fileBytes, contentType, fileName);
                }
                else
                {
                    TempData["Error"] = "Error al generar el archivo Excel";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al exportar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Exportar Nómina Vigente a Excel
        private async Task<byte[]> ExportarNominaExcel(string departmentId, DateTime fecha)
        {
            try
            {
                // Obtener empleados con sus datos básicos
                var empleados = await (from de in _context.DeptEmps
                                       join s in _context.Salaries on de.emp_no equals s.emp_no
                                       join e in _context.Employees on de.emp_no equals e.emp_no
                                       join d in _context.Departments on de.dept_no equals d.dept_no
                                       where de.from_date <= fecha &&
                                             (de.to_date == null || de.to_date >= fecha) &&
                                             s.from_date <= fecha &&
                                             (s.to_date == null || s.to_date >= fecha) &&
                                             e.IsActive && d.IsActive
                                       select new
                                       {
                                           Departamento = d.dept_name,
                                           DeptNo = d.dept_no,
                                           CI = e.ci,
                                           Nombre = e.first_name,
                                           Apellido = e.last_name,
                                           Email = e.correo,
                                           FechaIngreso = e.hire_date,
                                           Salario = s.salary,
                                           EstadoSalario = s.to_date == null ? "Vigente" : $"Hasta {s.to_date:dd/MM/yyyy}",
                                           EmpNo = e.emp_no
                                       }).ToListAsync();

                // Filtrar por departamento si es necesario
                if (!string.IsNullOrEmpty(departmentId))
                {
                    empleados = empleados.Where(x => x.DeptNo == departmentId).ToList();
                }

                // Obtener todos los títulos activos en la fecha dada
                var titulos = await _context.Titles
                    .Where(t => t.from_date <= fecha &&
                               (t.to_date == null || t.to_date >= fecha))
                    .ToDictionaryAsync(t => t.emp_no, t => t.title);

                // Crear resultados
                var resultados = new List<object[]>();
                foreach (var item in empleados)
                {
                    resultados.Add(new object[]
                    {
                item.Departamento,
                item.CI,
                item.Nombre,
                item.Apellido,
                item.Email,
                titulos.ContainsKey(item.EmpNo) ? titulos[item.EmpNo] : "Sin cargo",
                item.FechaIngreso.ToString("dd/MM/yyyy"),
                item.Salario.ToString("C2"),
                item.EstadoSalario
                    });
                }

                var headers = new[] { "Departamento", "CI", "Nombre", "Apellido", "Email", "Cargo", "Fecha Ingreso", "Salario", "Estado" };

                return ExcelHelper.GenerateExcel($"Nómina Vigente al {fecha:dd/MM/yyyy}", headers, resultados, r => r);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar nómina: {ex.Message}");
            }
        }

        // Exportar Cambios Salariales a Excel
        private async Task<byte[]> ExportarCambiosExcel(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var logs = await _context.LogAuditoriaSalarios
                    .Include(l => l.Employee)
                    .Where(l => l.fechaActualizacion.Date >= fechaInicio.Date &&
                               l.fechaActualizacion.Date <= fechaFin.Date)
                    .OrderByDescending(l => l.fechaActualizacion)
                    .ToListAsync();

                var resultados = new List<object[]>();
                foreach (var item in logs)
                {
                    resultados.Add(new object[]
                    {
                item.fechaActualizacion.ToString("dd/MM/yyyy HH:mm"),
                item.usuario,
                $"{item.Employee?.first_name} {item.Employee?.last_name}",
                item.Employee?.ci ?? "",
                item.DetalleCambio,
                item.salario.ToString("C2")
                    });
                }

                var headers = new[] { "Fecha", "Usuario", "Empleado", "CI", "Detalle", "Monto" };

                return ExcelHelper.GenerateExcel($"Cambios Salariales ({fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy})",
                    headers, resultados, r => r);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar cambios salariales: {ex.Message}");
            }
        }

        // Exportar Estructura Organizacional a Excel
        private async Task<byte[]> ExportarEstructuraExcel()
        {
            try
            {
                var estructura = await _context.Departments
                    .Where(d => d.IsActive)
                    .Select(d => new
                    {
                        Departamento = d.dept_name,
                        Gerente = _context.DeptManagers
                            .Where(dm => dm.dept_no == d.dept_no && dm.to_date == null)
                            .Select(dm => $"{dm.Employee.first_name} {dm.Employee.last_name}")
                            .FirstOrDefault() ?? "Sin gerente",
                        CantidadEmpleados = _context.DeptEmps
                            .Count(de => de.dept_no == d.dept_no && de.to_date == null),
                        TotalNomina = _context.Salaries
                            .Where(s => s.to_date == null &&
                                       _context.DeptEmps.Any(de => de.emp_no == s.emp_no &&
                                                                  de.dept_no == d.dept_no &&
                                                                  de.to_date == null))
                            .Sum(s => (decimal?)s.salary) ?? 0
                    })
                    .ToListAsync();

                var resultados = new List<object[]>();
                foreach (var item in estructura)
                {
                    resultados.Add(new object[]
                    {
                item.Departamento,
                item.Gerente,
                item.CantidadEmpleados,
                item.TotalNomina.ToString("C2")
                    });
                }

                var headers = new[] { "Departamento", "Gerente", "Cantidad Empleados", "Total Nómina" };

                return ExcelHelper.GenerateExcel("Estructura Organizacional", headers, resultados, r => r);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar estructura: {ex.Message}");
            }
        }

        // Exportar Auditoría a Excel
        private async Task<byte[]> ExportarAuditoriaExcel(DateTime fechaInicio, DateTime fechaFin, string usuario)
        {
            try
            {
                var query = _context.LogAuditoriaSalarios
                    .Include(l => l.Employee)
                    .Where(l => l.fechaActualizacion.Date >= fechaInicio.Date &&
                               l.fechaActualizacion.Date <= fechaFin.Date);

                if (!string.IsNullOrEmpty(usuario))
                {
                    query = query.Where(l => l.usuario == usuario);
                }

                var logs = await query.OrderByDescending(l => l.fechaActualizacion).ToListAsync();

                var resultados = new List<object[]>();
                foreach (var item in logs)
                {
                    resultados.Add(new object[]
                    {
                item.fechaActualizacion.ToString("dd/MM/yyyy HH:mm"),
                item.usuario,
                $"{item.Employee?.first_name} {item.Employee?.last_name}",
                item.Employee?.ci ?? "",
                item.DetalleCambio.Length > 50 ? item.DetalleCambio.Substring(0, 50) + "..." : item.DetalleCambio,
                item.salario.ToString("C2")
                    });
                }

                var headers = new[] { "Fecha", "Usuario", "Empleado", "CI", "Acción", "Monto" };

                return ExcelHelper.GenerateExcel($"Auditoría ({fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy})",
                    headers, resultados, r => r);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar auditoría: {ex.Message}");
            }
        }

        #endregion

        #region Exportación a PDF

        // GET: Reports/ExportarPDF
        public async Task<IActionResult> ExportarPDF(string tipo, string departmentId, DateTime? fecha,
                                                       DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                byte[] fileBytes = null;
                string fileName = "";

                switch (tipo?.ToLower())
                {
                    case "nomina":
                        fileBytes = await ExportarNominaPDF(departmentId, fecha ?? DateTime.Today);
                        fileName = $"NominaVigente_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        break;

                    case "cambios":
                        fileBytes = await ExportarCambiosPDF(fechaInicio ?? DateTime.Today.AddMonths(-1),
                                                             fechaFin ?? DateTime.Today);
                        fileName = $"CambiosSalariales_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        break;

                    case "estructura":
                        fileBytes = await ExportarEstructuraPDF();
                        fileName = $"EstructuraOrganizacional_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        break;

                    default:
                        TempData["Error"] = "Tipo de reporte no válido para PDF";
                        return RedirectToAction(nameof(Index));
                }

                if (fileBytes != null)
                {
                    return File(fileBytes, "application/pdf", fileName);
                }
                else
                {
                    TempData["Error"] = "Error al generar el archivo PDF";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al exportar a PDF: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Exportar Nómina Vigente a PDF
        private async Task<byte[]> ExportarNominaPDF(string departmentId, DateTime fecha)
        {
            try
            {
                var empleados = await (from de in _context.DeptEmps
                                       join s in _context.Salaries on de.emp_no equals s.emp_no
                                       join e in _context.Employees on de.emp_no equals e.emp_no
                                       join d in _context.Departments on de.dept_no equals d.dept_no
                                       where de.from_date <= fecha &&
                                             (de.to_date == null || de.to_date >= fecha) &&
                                             s.from_date <= fecha &&
                                             (s.to_date == null || s.to_date >= fecha) &&
                                             e.IsActive && d.IsActive
                                       select new
                                       {
                                           Departamento = d.dept_name,
                                           DeptNo = d.dept_no,
                                           CI = e.ci,
                                           Nombre = e.first_name,
                                           Apellido = e.last_name,
                                           Email = e.correo,
                                           FechaIngreso = e.hire_date,
                                           Salario = s.salary,
                                           EmpNo = e.emp_no
                                       }).ToListAsync();

                if (!string.IsNullOrEmpty(departmentId))
                {
                    empleados = empleados.Where(x => x.DeptNo == departmentId).ToList();
                }

                var titulos = await _context.Titles
                    .Where(t => t.from_date <= fecha &&
                               (t.to_date == null || t.to_date >= fecha))
                    .ToDictionaryAsync(t => t.emp_no, t => t.title);

                var rows = new List<string[]>();
                foreach (var item in empleados)
                {
                    rows.Add(new string[]
                    {
                item.Departamento,
                item.CI,
                $"{item.Nombre} {item.Apellido}",
                item.Email,
                titulos.ContainsKey(item.EmpNo) ? titulos[item.EmpNo] : "Sin cargo",
                item.FechaIngreso.ToString("dd/MM/yyyy"),
                item.Salario.ToString("C2")
                    });
                }

                var headers = new[] { "Departamento", "CI", "Nombre Completo", "Email", "Cargo", "Fecha Ingreso", "Salario" };

                var totalNomina = empleados.Sum(x => x.Salario).ToString("C2");
                rows.Add(new string[] { "", "", "", "", "", "TOTAL NÓMINA:", totalNomina });

                var html = PdfHelper.GenerateHtmlTable($"Nómina Vigente al {fecha:dd/MM/yyyy}", headers, rows);
                return PdfHelper.GeneratePdf(html, "Nómina Vigente");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar nómina a PDF: {ex.Message}");
            }
        }

        // Exportar Cambios Salariales a PDF
        private async Task<byte[]> ExportarCambiosPDF(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var logs = await _context.LogAuditoriaSalarios
                    .Include(l => l.Employee)
                    .Where(l => l.fechaActualizacion.Date >= fechaInicio.Date &&
                               l.fechaActualizacion.Date <= fechaFin.Date)
                    .OrderByDescending(l => l.fechaActualizacion)
                    .ToListAsync();

                var rows = new List<string[]>();
                foreach (var item in logs)
                {
                    rows.Add(new string[]
                    {
                item.fechaActualizacion.ToString("dd/MM/yyyy HH:mm"),
                item.usuario,
                $"{item.Employee?.first_name} {item.Employee?.last_name}",
                item.DetalleCambio.Length > 50 ? item.DetalleCambio.Substring(0, 50) + "..." : item.DetalleCambio,
                item.salario.ToString("C2")
                    });
                }

                var headers = new[] { "Fecha", "Usuario", "Empleado", "Detalle", "Monto" };

                var html = PdfHelper.GenerateHtmlTable($"Cambios Salariales ({fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy})",
                    headers, rows);
                return PdfHelper.GeneratePdf(html, "Cambios Salariales");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar cambios a PDF: {ex.Message}");
            }
        }

        // Exportar Estructura Organizacional a PDF
        private async Task<byte[]> ExportarEstructuraPDF()
        {
            try
            {
                var estructura = await _context.Departments
                    .Where(d => d.IsActive)
                    .Select(d => new
                    {
                        Departamento = d.dept_name,
                        Gerente = _context.DeptManagers
                            .Where(dm => dm.dept_no == d.dept_no && dm.to_date == null)
                            .Select(dm => $"{dm.Employee.first_name} {dm.Employee.last_name}")
                            .FirstOrDefault() ?? "Sin gerente",
                        CantidadEmpleados = _context.DeptEmps
                            .Count(de => de.dept_no == d.dept_no && de.to_date == null)
                    })
                    .ToListAsync();

                var rows = new List<string[]>();
                foreach (var item in estructura)
                {
                    rows.Add(new string[]
                    {
                item.Departamento,
                item.Gerente,
                item.CantidadEmpleados.ToString()
                    });
                }

                var headers = new[] { "Departamento", "Gerente", "Cantidad Empleados" };

                var html = PdfHelper.GenerateHtmlTable("Estructura Organizacional", headers, rows);
                return PdfHelper.GeneratePdf(html, "Estructura Organizacional");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar estructura a PDF: {ex.Message}");
            }
        }

        #endregion

        #region Gráficos / Datos para AJAX

        // GET: Reports/GetDistribucionSalarial
        public async Task<IActionResult> GetDistribucionSalarial()
        {
            var data = await _context.Departments
                .Where(d => d.IsActive)
                .Select(d => new
                {
                    Departamento = d.dept_name,
                    TotalNomina = _context.Salaries
                        .Where(s => s.to_date == null &&
                                   _context.DeptEmps.Any(de => de.emp_no == s.emp_no &&
                                                               de.dept_no == d.dept_no &&
                                                               de.to_date == null))
                        .Sum(s => (decimal?)s.salary) ?? 0,
                    CantidadEmpleados = _context.DeptEmps
                        .Count(de => de.dept_no == d.dept_no && de.to_date == null)
                })
                .Where(d => d.CantidadEmpleados > 0)
                .ToListAsync();

            return Json(data);
        }

        // GET: Reports/GetEvolucionSalarial
        public async Task<IActionResult> GetEvolucionSalarial(int meses = 6)
        {
            var fechaInicio = DateTime.Today.AddMonths(-meses);

            var logs = await _context.LogAuditoriaSalarios
                .Where(l => l.fechaActualizacion >= fechaInicio)
                .GroupBy(l => new { l.fechaActualizacion.Year, l.fechaActualizacion.Month })
                .Select(g => new
                {
                    Periodo = $"{g.Key.Year}-{g.Key.Month:D2}",
                    CantidadCambios = g.Count(),
                    MontoPromedio = g.Average(l => l.salario)
                })
                .OrderBy(g => g.Periodo)
                .ToListAsync();

            return Json(logs);
        }

        #endregion

        #region Métodos Privados

        private async Task LogActivity(string accion, string detalles)
        {
            try
            {
                var userName = _httpContextAccessor.HttpContext?.Session.GetString("UserName") ?? "Sistema";

                // Aquí podrías guardar en una tabla de logs de actividad si lo deseas
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {userName} - {accion} - {detalles}");
            }
            catch
            {
                // Ignorar errores de logging
            }
        }

        #endregion
    }
}