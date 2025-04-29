using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommuniCare.Migrations
{
    /// <inheritdoc />
    public partial class CoisasNovas8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemEmprestimo_Utilizador",
                table: "ItemEmprestimoUtilizador");

            migrationBuilder.AddColumn<int>(
                name: "itemEmpID",
                table: "ItemEmprestimoUtilizador",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "EmprestimoId",
                table: "ItemEmprestimoUtilizador",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemEmprestimo_Utilizador",
                table: "ItemEmprestimoUtilizador",
                column: "itemEmpID");

            migrationBuilder.CreateIndex(
                name: "IX_ItemEmprestimoUtilizador_EmprestimoId",
                table: "ItemEmprestimoUtilizador",
                column: "EmprestimoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemEmprestimoUtilizador_itemID",
                table: "ItemEmprestimoUtilizador",
                column: "itemID");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemEmprestimoUtilizador_Emprestimo_EmprestimoId",
                table: "ItemEmprestimoUtilizador",
                column: "EmprestimoId",
                principalTable: "Emprestimo",
                principalColumn: "emprestimoID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemEmprestimoUtilizador_Emprestimo_EmprestimoId",
                table: "ItemEmprestimoUtilizador");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemEmprestimo_Utilizador",
                table: "ItemEmprestimoUtilizador");

            migrationBuilder.DropIndex(
                name: "IX_ItemEmprestimoUtilizador_EmprestimoId",
                table: "ItemEmprestimoUtilizador");

            migrationBuilder.DropIndex(
                name: "IX_ItemEmprestimoUtilizador_itemID",
                table: "ItemEmprestimoUtilizador");

            migrationBuilder.DropColumn(
                name: "itemEmpID",
                table: "ItemEmprestimoUtilizador");

            migrationBuilder.DropColumn(
                name: "EmprestimoId",
                table: "ItemEmprestimoUtilizador");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemEmprestimo_Utilizador",
                table: "ItemEmprestimoUtilizador",
                columns: new[] { "itemID", "utilizadorID" });
        }
    }
}
