using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaNominaMVC.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    dept_no = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    dept_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.dept_no);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    emp_no = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ci = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    birth_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    gender = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    hire_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.emp_no);
                });

            migrationBuilder.CreateTable(
                name: "DeptEmps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    emp_no = table.Column<int>(type: "int", nullable: false),
                    dept_no = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    from_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    to_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeptEmps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeptEmps_Departments_dept_no",
                        column: x => x.dept_no,
                        principalTable: "Departments",
                        principalColumn: "dept_no",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeptEmps_Employees_emp_no",
                        column: x => x.emp_no,
                        principalTable: "Employees",
                        principalColumn: "emp_no",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeptManagers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    emp_no = table.Column<int>(type: "int", nullable: false),
                    dept_no = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    from_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    to_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeptManagers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeptManagers_Departments_dept_no",
                        column: x => x.dept_no,
                        principalTable: "Departments",
                        principalColumn: "dept_no",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeptManagers_Employees_emp_no",
                        column: x => x.emp_no,
                        principalTable: "Employees",
                        principalColumn: "emp_no",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogAuditoriaSalarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    fechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DetalleCambio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    salario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    emp_no = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogAuditoriaSalarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogAuditoriaSalarios_Employees_emp_no",
                        column: x => x.emp_no,
                        principalTable: "Employees",
                        principalColumn: "emp_no",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Salaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    emp_no = table.Column<int>(type: "int", nullable: false),
                    salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    from_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    to_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Salaries_Employees_emp_no",
                        column: x => x.emp_no,
                        principalTable: "Employees",
                        principalColumn: "emp_no",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Titles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    emp_no = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    from_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    to_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Titles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Titles_Employees_emp_no",
                        column: x => x.emp_no,
                        principalTable: "Employees",
                        principalColumn: "emp_no",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    emp_no = table.Column<int>(type: "int", nullable: false),
                    usuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    clave = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Employees_emp_no",
                        column: x => x.emp_no,
                        principalTable: "Employees",
                        principalColumn: "emp_no",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_dept_name",
                table: "Departments",
                column: "dept_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeptEmps_dept_no",
                table: "DeptEmps",
                column: "dept_no");

            migrationBuilder.CreateIndex(
                name: "IX_DeptEmps_emp_no",
                table: "DeptEmps",
                column: "emp_no");

            migrationBuilder.CreateIndex(
                name: "IX_DeptManagers_dept_no",
                table: "DeptManagers",
                column: "dept_no");

            migrationBuilder.CreateIndex(
                name: "IX_DeptManagers_emp_no",
                table: "DeptManagers",
                column: "emp_no");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ci",
                table: "Employees",
                column: "ci",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_correo",
                table: "Employees",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogAuditoriaSalarios_emp_no",
                table: "LogAuditoriaSalarios",
                column: "emp_no");

            migrationBuilder.CreateIndex(
                name: "IX_Salaries_emp_no",
                table: "Salaries",
                column: "emp_no");

            migrationBuilder.CreateIndex(
                name: "IX_Titles_emp_no",
                table: "Titles",
                column: "emp_no");

            migrationBuilder.CreateIndex(
                name: "IX_Users_emp_no",
                table: "Users",
                column: "emp_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_usuario",
                table: "Users",
                column: "usuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeptEmps");

            migrationBuilder.DropTable(
                name: "DeptManagers");

            migrationBuilder.DropTable(
                name: "LogAuditoriaSalarios");

            migrationBuilder.DropTable(
                name: "Salaries");

            migrationBuilder.DropTable(
                name: "Titles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
