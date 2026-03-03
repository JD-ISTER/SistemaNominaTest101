using System.ComponentModel.DataAnnotations;

namespace SistemaNominaMVC.Models.ViewModels
{
    public class SalaryCreateViewModel
    {
        public int? EmployeeId { get; set; }

        [Required(ErrorMessage = "El empleado es requerido")]
        [Display(Name = "Empleado")]
        public int emp_no { get; set; }

        [Required(ErrorMessage = "El salario es requerido")]
        [Display(Name = "Salario")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El salario debe ser mayor a 0")]
        [DataType(DataType.Currency)]
        public decimal salary { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime from_date { get; set; } = DateTime.Today;

        // Propiedades adicionales para la vista
        [Display(Name = "Nombre del Empleado")]
        public string? EmployeeName { get; set; }

        [Display(Name = "Salario Actual")]
        [DataType(DataType.Currency)]
        public decimal? CurrentSalary { get; set; }

        [Display(Name = "Vigente Desde")]
        [DataType(DataType.Date)]
        public DateTime? CurrentSalaryFromDate { get; set; }
    }
}