using System;

namespace SistemaNominaMVC.Models.ViewModels
{
    public class CambioSalarialViewModel
    {
        public int Id { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public string ApellidoEmpleado { get; set; } = string.Empty;
        public string CIEmpleado { get; set; } = string.Empty;
        public string DetalleCambio { get; set; } = string.Empty;
        public decimal Salario { get; set; }
        public decimal? SalarioAnterior { get; set; }
        public decimal? Incremento { get; set; }
        public string TipoCambio { get; set; } = string.Empty;
    }
}