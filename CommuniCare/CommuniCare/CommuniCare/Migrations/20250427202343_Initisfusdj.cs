using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommuniCare.Migrations
{
    /// <inheritdoc />
    public partial class Initisfusdj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    ArtigoId = table.Column<int>(type: "int", nullable: false),
                    AddedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => new { x.UtilizadorId, x.ArtigoId });
                    table.ForeignKey(
                        name: "FK_Favoritos_Artigo_ArtigoId",
                        column: x => x.ArtigoId,
                        principalTable: "Artigo",
                        principalColumn: "artigoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Utilizador_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizador",
                        principalColumn: "utilizadorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_ArtigoId",
                table: "Favoritos",
                column: "ArtigoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favoritos");
        }
    }
}
