using System.ComponentModel.DataAnnotations;

namespace SistemaNominaMVC.Models
{
    public class Department
    {
        [Key]
        [StringLength(4)]
        [Display(Name = "Código Departamento")]
        public string dept_no { get; set; } = string.Empty;  // Inicializado

        [Required]
        [StringLength(50)]
        [Display(Name = "Nombre Departamento")]
        public string dept_name { get; set; } = string.Empty;  // Inicializado

        public bool IsActive { get; set; } = true;

        // Propiedades de navegación - Inicializadas como colecciones vacías
        public virtual ICollection<DeptEmp> DeptEmps { get; set; } = new List<DeptEmp>();
        public virtual ICollection<DeptManager> DeptManagers { get; set; } = new List<DeptManager>();
    }
}