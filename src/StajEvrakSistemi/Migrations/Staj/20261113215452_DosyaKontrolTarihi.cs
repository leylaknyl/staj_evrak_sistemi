using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajEvrakSistemi.Migrations.Staj
{
    /// <inheritdoc />
    public partial class DosyaKontrolTarihi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "KontrolEdilmeUtc",
                table: "BasvuruDosyalari",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KontrolEdilmeUtc",
                table: "BasvuruDosyalari");
        }
    }
}
