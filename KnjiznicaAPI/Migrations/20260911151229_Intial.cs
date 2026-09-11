using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnjiznicaAPI.Migrations
{
    /// <inheritdoc />
    public partial class Intial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoriKnjiga",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    imeAutora = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    godinaRodenja = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoriKnjiga", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zanrovi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    imeZanra = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zanrovi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Knjige",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nazivKnjige = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AutorKnjigeId = table.Column<int>(type: "int", nullable: false),
                    datumUnosa = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Knjige", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Knjige_AutoriKnjiga_AutorKnjigeId",
                        column: x => x.AutorKnjigeId,
                        principalTable: "AutoriKnjiga",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KnjigeZanrovi",
                columns: table => new
                {
                    KnjigeId = table.Column<int>(type: "int", nullable: false),
                    ZanroviId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnjigeZanrovi", x => new { x.KnjigeId, x.ZanroviId });
                    table.ForeignKey(
                        name: "FK_KnjigeZanrovi_Knjige_KnjigeId",
                        column: x => x.KnjigeId,
                        principalTable: "Knjige",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KnjigeZanrovi_Zanrovi_ZanroviId",
                        column: x => x.ZanroviId,
                        principalTable: "Zanrovi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Knjige_AutorKnjigeId",
                table: "Knjige",
                column: "AutorKnjigeId");

            migrationBuilder.CreateIndex(
                name: "IX_KnjigeZanrovi_ZanroviId",
                table: "KnjigeZanrovi",
                column: "ZanroviId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnjigeZanrovi");

            migrationBuilder.DropTable(
                name: "Knjige");

            migrationBuilder.DropTable(
                name: "Zanrovi");

            migrationBuilder.DropTable(
                name: "AutoriKnjiga");
        }
    }
}
