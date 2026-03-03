using Microsoft.EntityFrameworkCore;
using SistemaNominaMVC.Models;

namespace SistemaNominaMVC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DeptEmp> DeptEmps { get; set; }
        public DbSet<DeptManager> DeptManagers { get; set; }
        public DbSet<Title> Titles { get; set; }
        public DbSet<Salary> Salaries { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LogAuditoriaSalario> LogAuditoriaSalarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar índices y relaciones
            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.ci)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.correo)
                .IsUnique();

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.dept_name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.usuario)
                .IsUnique();

            // Relaciones
            modelBuilder.Entity<DeptEmp>()
                .HasOne(de => de.Employee)
                .WithMany(e => e.DeptEmps)
                .HasForeignKey(de => de.emp_no);

            modelBuilder.Entity<DeptEmp>()
                .HasOne(de => de.Department)
                .WithMany(d => d.DeptEmps)
                .HasForeignKey(de => de.dept_no);

            modelBuilder.Entity<DeptManager>()
                .HasOne(dm => dm.Employee)
                .WithMany(e => e.DeptManagers)
                .HasForeignKey(dm => dm.emp_no);

            modelBuilder.Entity<DeptManager>()
                .HasOne(dm => dm.Department)
                .WithMany(d => d.DeptManagers)
                .HasForeignKey(dm => dm.dept_no);

            modelBuilder.Entity<Title>()
                .HasOne(t => t.Employee)
                .WithMany(e => e.Titles)
                .HasForeignKey(t => t.emp_no);

            modelBuilder.Entity<Salary>()
                .HasOne(s => s.Employee)
                .WithMany(e => e.Salaries)
                .HasForeignKey(s => s.emp_no);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithOne(e => e.User)
                .HasForeignKey<User>(u => u.emp_no);

            modelBuilder.Entity<LogAuditoriaSalario>()
                .HasOne(l => l.Employee)
                .WithMany()
                .HasForeignKey(l => l.emp_no);
        }
    }
}