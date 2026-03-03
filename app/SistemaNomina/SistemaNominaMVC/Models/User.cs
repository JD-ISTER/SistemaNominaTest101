using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaNominaMVC.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public int emp_no { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Usuario")]
        public string usuario { get; set; } = string.Empty;  // Inicializado

        [Required]
        [StringLength(255)]
        public string clave { get; set; } = string.Empty;  // Inicializado

        [Required]
        [StringLength(20)]
        [Display(Name = "Rol")]
        public string rol { get; set; } = string.Empty;  // Inicializado

        public bool IsActive { get; set; } = true;

        // Propiedad de navegación - Puede ser null
        public virtual Employee? Employee { get; set; }
    }
}