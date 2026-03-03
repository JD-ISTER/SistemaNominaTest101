using System;

namespace SistemaNominaMVC.Models.ViewModels
{
    public class NominaVigenteViewModel
    {
        public int EmpleadoId { get; set; }
        public string CIEmpleado { get; set; } = string.Empty;
        public string NombreEmpleado { get; set; } = string.Empty;
        public string ApellidoEmpleado { get; set; } = string.Empty;
        public string EmailEmpleado { get; set; } = string.Empty;
        public string TituloEmpleado { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public decimal Salario { get; set; }
        public DateTime FechaInicioSalario { get; set; }
        public DateTime? FechaFinSalario { get; set; }
        public bool EsSalarioVigente { get; set; }
        public string Departamento { get; set; } = string.Empty;
        public string CodigoDepartamento { get; set; } = string.Empty;
    }
}