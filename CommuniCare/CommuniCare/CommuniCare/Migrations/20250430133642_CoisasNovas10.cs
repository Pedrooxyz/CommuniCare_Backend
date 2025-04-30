using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommuniCare.Migrations
{
    /// <inheritdoc />
    public partial class CoisasNovas10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotografiaPA",
                table: "PedidosAjuda",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotografiaItem",
                table: "ItemEmprestimo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotografiaArt",
                table: "Artigo",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotografiaPA",
                table: "PedidosAjuda");

            migrationBuilder.DropColumn(
                name: "FotografiaItem",
                table: "ItemEmprestimo");

            migrationBuilder.DropColumn(
                name: "FotografiaArt",
                table: "Artigo");
        }
    }
}
