using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GesFer.Admin.Infra.Data.Migrations
{
    /// <summary>
    /// Añade a la tabla Logs las columnas Source, CompanyId y UserId cuando la tabla
    /// existe con esquema mínimo (p. ej. creada por Serilog) y no las tiene.
    /// Idempotente: solo añade cada columna si no existe (para no fallar cuando
    /// CreateLogsTableIfNotExists ya creó la tabla con esquema completo).
    /// </summary>
    public partial class AddMissingColumnsToLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotente: procedimiento almacenado para añadir columnas solo si no existen (MySQL no tiene ADD COLUMN IF NOT EXISTS)
            var proc = @"
DROP PROCEDURE IF EXISTS add_logs_missing_columns;
CREATE PROCEDURE add_logs_missing_columns()
BEGIN
    IF (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Logs' AND COLUMN_NAME = 'Source') = 0 THEN
        ALTER TABLE Logs ADD COLUMN Source longtext NULL CHARACTER SET utf8mb4;
    END IF;
    IF (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Logs' AND COLUMN_NAME = 'CompanyId') = 0 THEN
        ALTER TABLE Logs ADD COLUMN CompanyId char(36) NULL COLLATE ascii_general_ci;
    END IF;
    IF (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Logs' AND COLUMN_NAME = 'UserId') = 0 THEN
        ALTER TABLE Logs ADD COLUMN UserId char(36) NULL COLLATE ascii_general_ci;
    END IF;
END;
";
            migrationBuilder.Sql(proc);
            migrationBuilder.Sql("CALL add_logs_missing_columns();");
            migrationBuilder.Sql("DROP PROCEDURE add_logs_missing_columns;");
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
