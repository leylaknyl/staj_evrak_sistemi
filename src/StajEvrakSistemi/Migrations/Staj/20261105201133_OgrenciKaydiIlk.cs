using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajEvrakSistemi.Migrations.Staj
{
    /// <inheritdoc />
    public partial class OgrenciKaydiIlk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OgrenciKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OgrenciNo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AdSoyad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Eposta = table.Column<string>(type: "TEXT", nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Bolum = table.Column<string>(type: "TEXT", nullable: false),
                    KimlikNo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Uyruk = table.Column<string>(type: "TEXT", nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Adres = table.Column<string>(type: "TEXT", nullable: false),
                    Sinif = table.Column<int>(type: "INTEGER", nullable: false),
                    OncekiStajGunu = table.Column<int>(type: "INTEGER", nullable: false),
                    SaglikYardimiAliyor = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciKayitlari", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciKayitlari_OgrenciNo",
                table: "OgrenciKayitlari",
                column: "OgrenciNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OgrenciKayitlari");
        }
    }
}
