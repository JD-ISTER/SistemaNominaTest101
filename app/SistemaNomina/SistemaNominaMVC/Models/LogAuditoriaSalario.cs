using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaNominaMVC.Models
{
    public class LogAuditoriaSalario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Usuario")]
        public string usuario { get; set; } = string.Empty;  // Inicializado

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha Actualización")]
        public DateTime fechaActualizacion { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Detalle Cambio")]
        public string DetalleCambio { get; set; } = string.Empty;  // Inicializado

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Salario")]
        public decimal salario { get; set; }

        [Required]
        [Display(Name = "Empleado")]
        public int emp_no { get; set; }

        // Propiedad de navegación - Puede ser null
        [ForeignKey("emp_no")]
        public virtual Employee? Employee { get; set; }
    }
}