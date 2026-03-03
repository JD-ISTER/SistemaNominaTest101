using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaNominaMVC.Models
{
    public class DeptManager
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public int emp_no { get; set; }

        [Required]
        [ForeignKey("Department")]
        public string dept_no { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha Inicio")]
        public DateTime from_date { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha Fin")]
        public DateTime? to_date { get; set; }

        // Propiedades de navegación
        public virtual Employee Employee { get; set; }
        public virtual Department Department { get; set; }
    }
}