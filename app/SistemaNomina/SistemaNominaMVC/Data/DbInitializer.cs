using SistemaNominaMVC.Models;
using SistemaNominaMVC.Helpers;

namespace SistemaNominaMVC.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Asegurar que la base de datos está creada
            context.Database.EnsureCreated();

            // Buscar si ya hay empleados
            if (context.Employees.Any())
            {
                return; // Base de datos ya tiene datos
            }

            // Crear departamentos
            var departments = new Department[]
            {
                new Department { dept_no = "D001", dept_name = "Dirección General", IsActive = true },
                new Department { dept_no = "D002", dept_name = "Recursos Humanos", IsActive = true },
                new Department { dept_no = "D003", dept_name = "Tecnología", IsActive = true },
                new Department { dept_no = "D004", dept_name = "Finanzas", IsActive = true },
                new Department { dept_no = "D005", dept_name = "Marketing", IsActive = true }
            };

            foreach (Department d in departments)
            {
                context.Departments.Add(d);
            }
            context.SaveChanges();

            // Crear empleados
            var employees = new Employee[]
            {
                new Employee {
                    ci = "12345678",
                    birth_date = new DateTime(1980, 1, 15),
                    first_name = "Admin",
                    last_name = "Sistema",
                    gender = "M",
                    hire_date = new DateTime(2020, 1, 1),
                    correo = "admin@sistema.com",
                    IsActive = true
                },
                new Employee {
                    ci = "87654321",
                    birth_date = new DateTime(1985, 5, 20),
                    first_name = "Recursos",
                    last_name = "Humanos",
                    gender = "F",
                    hire_date = new DateTime(2021, 3, 15),
                    correo = "rrhh@sistema.com",
                    IsActive = true
                },
                new Employee {
                    ci = "11223344",
                    birth_date = new DateTime(1990, 8, 10),
                    first_name = "Juan",
                    last_name = "Pérez",
                    gender = "M",
                    hire_date = new DateTime(2022, 6, 1),
                    correo = "juan.perez@sistema.com",
                    IsActive = true
                },
                new Employee {
                    ci = "44332211",
                    birth_date = new DateTime(1992, 3, 25),
                    first_name = "María",
                    last_name = "González",
                    gender = "F",
                    hire_date = new DateTime(2022, 7, 15),
                    correo = "maria.gonzalez@sistema.com",
                    IsActive = true
                }
            };

            foreach (Employee e in employees)
            {
                context.Employees.Add(e);
            }
            context.SaveChanges();

            // Crear usuarios
            var users = new User[]
            {
                new User {
                    emp_no = 1, // Admin
                    usuario = "admin",
                    clave = PasswordHelper.HashPassword("123456"),
                    rol = "Administrador",
                    IsActive = true
                },
                new User {
                    emp_no = 2, // RRHH
                    usuario = "rrhh",
                    clave = PasswordHelper.HashPassword("123456"),
                    rol = "RRHH",
                    IsActive = true
                }
            };

            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();

            // Crear salarios
            var salaries = new Salary[]
            {
                new Salary {
                    emp_no = 1,
                    salary = 5000.00m,
                    from_date = new DateTime(2020, 1, 1),
                    to_date = null // Salario actual
                },
                new Salary {
                    emp_no = 2,
                    salary = 3500.00m,
                    from_date = new DateTime(2021, 3, 15),
                    to_date = null
                },
                new Salary {
                    emp_no = 3,
                    salary = 2800.00m,
                    from_date = new DateTime(2022, 6, 1),
                    to_date = null
                },
                new Salary {
                    emp_no = 4,
                    salary = 2900.00m,
                    from_date = new DateTime(2022, 7, 15),
                    to_date = null
                }
            };

            foreach (Salary s in salaries)
            {
                context.Salaries.Add(s);
            }
            context.SaveChanges();

            // Crear títulos
            var titles = new Title[]
            {
                new Title {
                    emp_no = 1,
                    title = "Director General",
                    from_date = new DateTime(2020, 1, 1),
                    to_date = null
                },
                new Title {
                    emp_no = 2,
                    title = "Gerente de RRHH",
                    from_date = new DateTime(2021, 3, 15),
                    to_date = null
                },
                new Title {
                    emp_no = 3,
                    title = "Desarrollador Senior",
                    from_date = new DateTime(2022, 6, 1),
                    to_date = null
                },
                new Title {
                    emp_no = 4,
                    title = "Diseñadora UX/UI",
                    from_date = new DateTime(2022, 7, 15),
                    to_date = null
                }
            };

            foreach (Title t in titles)
            {
                context.Titles.Add(t);
            }
            context.SaveChanges();

            // Asignaciones a departamentos
            var deptEmps = new DeptEmp[]
            {
                new DeptEmp {
                    emp_no = 1,
                    dept_no = "D001",
                    from_date = new DateTime(2020, 1, 1),
                    to_date = null
                },
                new DeptEmp {
                    emp_no = 2,
                    dept_no = "D002",
                    from_date = new DateTime(2021, 3, 15),
                    to_date = null
                },
                new DeptEmp {
                    emp_no = 3,
                    dept_no = "D003",
                    from_date = new DateTime(2022, 6, 1),
                    to_date = null
                },
                new DeptEmp {
                    emp_no = 4,
                    dept_no = "D003",
                    from_date = new DateTime(2022, 7, 15),
                    to_date = null
                }
            };

            foreach (DeptEmp de in deptEmps)
            {
                context.DeptEmps.Add(de);
            }
            context.SaveChanges();

            // Gerentes de departamento
            var deptManagers = new DeptManager[]
            {
                new DeptManager {
                    emp_no = 1,
                    dept_no = "D001",
                    from_date = new DateTime(2020, 1, 1),
                    to_date = null
                },
                new DeptManager {
                    emp_no = 2,
                    dept_no = "D002",
                    from_date = new DateTime(2021, 3, 15),
                    to_date = null
                }
            };

            foreach (DeptManager dm in deptManagers)
            {
                context.DeptManagers.Add(dm);
            }
            context.SaveChanges();
        }
    }
}