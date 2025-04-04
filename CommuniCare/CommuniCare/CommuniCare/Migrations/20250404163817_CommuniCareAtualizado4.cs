using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommuniCare.Migrations
{
    /// <inheritdoc />
    public partial class CommuniCareAtualizado4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FKMorada368657",
                table: "Morada");

            migrationBuilder.DropPrimaryKey(
                name: "PK__CP__F5B22BE63663255C",
                table: "CP");

            migrationBuilder.DropColumn(
                name: "CPID",
                table: "CP");

            migrationBuilder.AlterColumn<string>(
                name: "CPID",
                table: "Morada",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CPostal",
                table: "CP",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK__CP__F5B22BE63663255C",
                table: "CP",
                column: "CPostal");

            migrationBuilder.AddForeignKey(
                name: "FKMorada368657",
                table: "Morada",
                column: "CPID",
                principalTable: "CP",
                principalColumn: "CPostal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FKMorada368657",
                table: "Morada");

            migrationBuilder.DropPrimaryKey(
                name: "PK__CP__F5B22BE63663255C",
                table: "CP");

            migrationBuilder.DropColumn(
                name: "CPostal",
                table: "CP");

            migrationBuilder.AlterColumn<int>(
                name: "CPID",
                table: "Morada",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CPID",
                table: "CP",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK__CP__F5B22BE63663255C",
                table: "CP",
                column: "CPID");

            migrationBuilder.AddForeignKey(
                name: "FKMorada368657",
                table: "Morada",
                column: "CPID",
                principalTable: "CP",
                principalColumn: "CPID");
        }
    }
}
