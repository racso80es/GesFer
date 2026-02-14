using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GesFer.Admin.Infra.Data.Migrations
{
    /// <summary>
    /// Crea la tabla Logs con esquema completo si no existe (idempotente).
    /// En nuevos entornos la tabla queda creada por Admin; Serilog solo escribe en ella.
    /// Debe ejecutarse antes de AddMissingColumnsToLogs (timestamp anterior).
    /// </summary>
    public partial class CreateLogsTableIfNotExists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
CREATE TABLE IF NOT EXISTS `Logs` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Level` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Message` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Template` longtext CHARACTER SET utf8mb4 NULL,
    `Exception` longtext CHARACTER SET utf8mb4 NULL,
    `Properties` longtext CHARACTER SET utf8mb4 NULL,
    `TimeStamp` datetime(6) NOT NULL,
    `Source` longtext CHARACTER SET utf8mb4 NULL,
    `CompanyId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
    `UserId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
    `ClientInfo` longtext CHARACTER SET utf8mb4 NULL,
    PRIMARY KEY (`Id`)
) CHARACTER SET utf8mb4;
";
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No DROP TABLE en Down: la tabla puede ser usada por Serilog u otros; 
            // revertir esta migración no debe eliminar datos.
            // migrationBuilder.DropTable(name: "Logs");
        }
    }
}
