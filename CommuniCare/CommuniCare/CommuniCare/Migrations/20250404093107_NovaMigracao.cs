using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommuniCare.Migrations
{
    /// <inheritdoc />
    public partial class NovaMigracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dataTransmissao",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "Distrito",
                table: "Morada");

            migrationBuilder.DropColumn(
                name: "descCP",
                table: "CP");

            migrationBuilder.AddColumn<DateTime>(
                name: "dataTransacao",
                table: "Transacao",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "descTU",
                table: "TipoUtilizador",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "conteudo",
                table: "Mensagem",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localidade",
                table: "CP",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dataTransacao",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "Localidade",
                table: "CP");

            migrationBuilder.AddColumn<int>(
                name: "dataTransmissao",
                table: "Transacao",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "descTU",
                table: "TipoUtilizador",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Distrito",
                table: "Morada",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "conteudo",
                table: "Mensagem",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "descCP",
                table: "CP",
                type: "int",
                nullable: true);
        }
    }
}
