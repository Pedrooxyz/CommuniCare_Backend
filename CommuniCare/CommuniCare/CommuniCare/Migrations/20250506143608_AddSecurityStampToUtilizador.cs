using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommuniCare.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityStampToUtilizador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "idEmprestador",
                table: "ItemEmprestimo",
                newName: "IdEmprestador");

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Utilizador",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Utilizador");

            migrationBuilder.RenameColumn(
                name: "IdEmprestador",
                table: "ItemEmprestimo",
                newName: "idEmprestador");
        }
    }
}
