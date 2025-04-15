using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommuniCare.Migrations
{
    /// <inheritdoc />
    public partial class CommuniCareAtualizado7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemEmprestimo_Utilizador");

            migrationBuilder.RenameTable(
                name: "TransacaoEmprestimos",
                newName: "TransacoesEmprestimo");

            migrationBuilder.AddColumn<int>(
                name: "idEmprestador",
                table: "ItemEmprestimo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ItemEmprestimoUtilizador",
                columns: table => new
                {
                    itemID = table.Column<int>(type: "int", nullable: false),
                    utilizadorID = table.Column<int>(type: "int", nullable: false),
                    tipoRelacao = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemEmprestimo_Utilizador", x => new { x.itemID, x.utilizadorID });
                    table.ForeignKey(
                        name: "FK_ItemEmprestimo_Utilizador_ItemEmprestimo",
                        column: x => x.itemID,
                        principalTable: "ItemEmprestimo",
                        principalColumn: "itemID");
                    table.ForeignKey(
                        name: "FK_ItemEmprestimo_Utilizador_Utilizador",
                        column: x => x.utilizadorID,
                        principalTable: "Utilizador",
                        principalColumn: "utilizadorID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemEmprestimoUtilizador_utilizadorID",
                table: "ItemEmprestimoUtilizador",
                column: "utilizadorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemEmprestimoUtilizador");

            migrationBuilder.DropColumn(
                name: "idEmprestador",
                table: "ItemEmprestimo");

            migrationBuilder.RenameTable(
                name: "TransacoesEmprestimo",
                newName: "TransacaoEmprestimos");

            migrationBuilder.CreateTable(
                name: "ItemEmprestimo_Utilizador",
                columns: table => new
                {
                    itemID = table.Column<int>(type: "int", nullable: false),
                    utilizadorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ItemEmpr__39C0F0DBD085C080", x => new { x.itemID, x.utilizadorID });
                    table.ForeignKey(
                        name: "FKItemEmpres234174",
                        column: x => x.utilizadorID,
                        principalTable: "Utilizador",
                        principalColumn: "utilizadorID");
                    table.ForeignKey(
                        name: "FKItemEmpres991231",
                        column: x => x.itemID,
                        principalTable: "ItemEmprestimo",
                        principalColumn: "itemID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemEmprestimo_Utilizador_utilizadorID",
                table: "ItemEmprestimo_Utilizador",
                column: "utilizadorID");
        }
    }
}
