using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaNominaMVC.Models
{
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int emp_no { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "CI/NIT")]
        public string ci { get; set; } = string.Empty;  // Inicializado

        [Required]
        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime birth_date { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Primer Nombre")]
        public string first_name { get; set; } = string.Empty;  // Inicializado

        [Required]
        [StringLength(50)]
        [Display(Name = "Apellido")]
        public string last_name { get; set; } = string.Empty;  // Inicializado

        [Required]
        [StringLength(1)]
        [Display(Name = "Género")]
        public string gender { get; set; } = string.Empty;  // Inicializado

        [Required]
        [Display(Name = "Fecha de Contratación")]
        [DataType(DataType.Date)]
        public DateTime hire_date { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Correo Electrónico")]
        public string correo { get; set; } = string.Empty;  // Inicializado

        public bool IsActive { get; set; } = true;

        // Propiedades de navegación - Inicializadas como colecciones vacías
        public virtual ICollection<DeptEmp> DeptEmps { get; set; } = new List<DeptEmp>();
        public virtual ICollection<DeptManager> DeptManagers { get; set; } = new List<DeptManager>();
        public virtual ICollection<Title> Titles { get; set; } = new List<Title>();
        public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();
        public virtual User? User { get; set; }  // Nullable porque puede no tener usuario
    }
}