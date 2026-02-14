using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GesFer.Admin.Infra.Data.Migrations
{
    /// <summary>
    /// Añade a la tabla Logs las columnas Source, CompanyId y UserId cuando la tabla
    /// existe con esquema mínimo (p. ej. creada por Serilog) y no las tiene.
    /// Si la tabla fue creada por EnsureCreated con el modelo completo, estas columnas
    /// ya existen; en MySQL no hay ADD COLUMN IF NOT EXISTS, aplicar solo en BD que lo requiera.
    /// </summary>
    public partial class AddMissingColumnsToLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Logs",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Logs",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Logs",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Source", table: "Logs");
            migrationBuilder.DropColumn(name: "CompanyId", table: "Logs");
            migrationBuilder.DropColumn(name: "UserId", table: "Logs");
        }
    }
}
