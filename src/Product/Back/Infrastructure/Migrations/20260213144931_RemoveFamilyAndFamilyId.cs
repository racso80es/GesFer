using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GesFer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFamilyAndFamilyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Families_FamilyId",
                table: "Articles");

            migrationBuilder.DropTable(
                name: "Families");

            migrationBuilder.DropIndex(
                name: "IX_Articles_FamilyId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "Articles");

            migrationBuilder.AlterColumn<Guid>(
                name: "ArticleFamilyId",
                table: "Articles",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ArticleFamilyId",
                table: "Articles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyId",
                table: "Articles",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Families",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CompanyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IvaPercentage = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Families_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_FamilyId",
                table: "Articles",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Families_CompanyId_Name",
                table: "Families",
                columns: new[] { "CompanyId", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Families_FamilyId",
                table: "Articles",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
