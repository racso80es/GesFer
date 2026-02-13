using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GesFer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleFamilyIdToArticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ArticleFamilyId",
                table: "Articles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_ArticleFamilyId",
                table: "Articles",
                column: "ArticleFamilyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_ArticleFamilies_ArticleFamilyId",
                table: "Articles",
                column: "ArticleFamilyId",
                principalTable: "ArticleFamilies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_ArticleFamilies_ArticleFamilyId",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_ArticleFamilyId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ArticleFamilyId",
                table: "Articles");
        }
    }
}
