using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajEvrakSistemi.Migrations.Staj
{
    /// <inheritdoc />
    public partial class BasvuruTablolari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Basvurular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OgrenciKaydiId = table.Column<int>(type: "INTEGER", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturulmaUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Basvurular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Basvurular_OgrenciKayitlari_OgrenciKaydiId",
                        column: x => x.OgrenciKaydiId,
                        principalTable: "OgrenciKayitlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BasvuruDosyalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BasvuruId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrijinalDosyaAdi = table.Column<string>(type: "TEXT", nullable: false),
                    DepolamaAnahtari = table.Column<string>(type: "TEXT", nullable: false),
                    BoyutBayt = table.Column<long>(type: "INTEGER", nullable: false),
                    YuklenmeUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasvuruDosyalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BasvuruDosyalari_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DogrulamaSonuclari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BasvuruDosyasiId = table.Column<int>(type: "INTEGER", nullable: false),
                    Alan = table.Column<string>(type: "TEXT", nullable: false),
                    Mesaj = table.Column<string>(type: "TEXT", nullable: false),
                    UyariMi = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DogrulamaSonuclari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DogrulamaSonuclari_BasvuruDosyalari_BasvuruDosyasiId",
                        column: x => x.BasvuruDosyasiId,
                        principalTable: "BasvuruDosyalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BasvuruDosyalari_BasvuruId",
                table: "BasvuruDosyalari",
                column: "BasvuruId");

            migrationBuilder.CreateIndex(
                name: "IX_Basvurular_OgrenciKaydiId",
                table: "Basvurular",
                column: "OgrenciKaydiId");

            migrationBuilder.CreateIndex(
                name: "IX_DogrulamaSonuclari_BasvuruDosyasiId",
                table: "DogrulamaSonuclari",
                column: "BasvuruDosyasiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DogrulamaSonuclari");

            migrationBuilder.DropTable(
                name: "BasvuruDosyalari");

            migrationBuilder.DropTable(
                name: "Basvurular");
        }
    }
}
