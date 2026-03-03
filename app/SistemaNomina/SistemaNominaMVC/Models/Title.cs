using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaNominaMVC.Models
{
    public class Title
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public int emp_no { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Título/Cargo")]
        public string title { get; set; } = string.Empty;  // Inicializado

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha Inicio")]
        public DateTime from_date { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha Fin")]
        public DateTime? to_date { get; set; }

        // Propiedad de navegación - Puede ser null
        public virtual Employee? Employee { get; set; }
    }
}