using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommuniCare.Migrations
{
    /// <inheritdoc />
    public partial class CoisasNovas9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemEmprestimoUtilizador_Emprestimo_EmprestimoId",
                table: "ItemEmprestimoUtilizador");

            migrationBuilder.RenameColumn(
                name: "EmprestimoId",
                table: "ItemEmprestimoUtilizador",
                newName: "emprestimoID");

            migrationBuilder.RenameIndex(
                name: "IX_ItemEmprestimoUtilizador_EmprestimoId",
                table: "ItemEmprestimoUtilizador",
                newName: "IX_ItemEmprestimoUtilizador_emprestimoID");

            migrationBuilder.AlterColumn<int>(
                name: "emprestimoID",
                table: "ItemEmprestimoUtilizador",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemEmprestimoUtilizador_Emprestimo_emprestimoID",
                table: "ItemEmprestimoUtilizador",
                column: "emprestimoID",
                principalTable: "Emprestimo",
                principalColumn: "emprestimoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemEmprestimoUtilizador_Emprestimo_emprestimoID",
                table: "ItemEmprestimoUtilizador");

            migrationBuilder.RenameColumn(
                name: "emprestimoID",
                table: "ItemEmprestimoUtilizador",
                newName: "EmprestimoId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemEmprestimoUtilizador_emprestimoID",
                table: "ItemEmprestimoUtilizador",
                newName: "IX_ItemEmprestimoUtilizador_EmprestimoId");

            migrationBuilder.AlterColumn<int>(
                name: "EmprestimoId",
                table: "ItemEmprestimoUtilizador",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemEmprestimoUtilizador_Emprestimo_EmprestimoId",
                table: "ItemEmprestimoUtilizador",
                column: "EmprestimoId",
                principalTable: "Emprestimo",
                principalColumn: "emprestimoID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
