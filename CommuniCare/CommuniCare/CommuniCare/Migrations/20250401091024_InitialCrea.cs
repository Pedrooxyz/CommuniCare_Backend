using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommuniCare.Migrations
{
    /// <inheritdoc />
    public partial class InitialCrea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CP",
                columns: table => new
                {
                    CPID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    descCP = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CP__F5B22BE63663255C", x => x.CPID);
                });

            migrationBuilder.CreateTable(
                name: "ItemEmprestimo",
                columns: table => new
                {
                    itemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nomeItem = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    descItem = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    disponivel = table.Column<byte>(type: "tinyint", nullable: true),
                    comissaoCares = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ItemEmpr__56A1284AB54FF227", x => x.itemID);
                });

            migrationBuilder.CreateTable(
                name: "Loja",
                columns: table => new
                {
                    lojaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nomeLoja = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    descLoja = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Loja__E00D7703297E42E3", x => x.lojaID);
                });

            migrationBuilder.CreateTable(
                name: "TipoContacto",
                columns: table => new
                {
                    tipoContactoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    descContacto = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TipoCont__CC154D73604D5E90", x => x.tipoContactoID);
                });

            migrationBuilder.CreateTable(
                name: "TipoUtilizador",
                columns: table => new
                {
                    tipoUtilizadorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    descTU = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TipoUtil__A479CAE646541112", x => x.tipoUtilizadorID);
                });

            migrationBuilder.CreateTable(
                name: "Transacao",
                columns: table => new
                {
                    transacaoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dataTransmissao = table.Column<int>(type: "int", nullable: true),
                    quantidade = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Transaca__EA9168CEEAA2B152", x => x.transacaoID);
                });

            migrationBuilder.CreateTable(
                name: "Morada",
                columns: table => new
                {
                    moradaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rua = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    numPorta = table.Column<int>(type: "int", nullable: true),
                    Distrito = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    CPID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Morada__5BDD9AB29A5F23DC", x => x.moradaID);
                    table.ForeignKey(
                        name: "FKMorada368657",
                        column: x => x.CPID,
                        principalTable: "CP",
                        principalColumn: "CPID");
                });

            migrationBuilder.CreateTable(
                name: "TransacaoAjuda",
                columns: table => new
                {
                    transacaoID = table.Column<int>(type: "int", nullable: false),
                    recetorTran = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Transaca__EA9168CE1D5A6645", x => x.transacaoID);
                    table.ForeignKey(
                        name: "FKTransacaoA7406",
                        column: x => x.transacaoID,
                        principalTable: "Transacao",
                        principalColumn: "transacaoID");
                });

            migrationBuilder.CreateTable(
                name: "TransacaoEmprestimos",
                columns: table => new
                {
                    transacaoID = table.Column<int>(type: "int", nullable: false),
                    nHoras = table.Column<int>(type: "int", nullable: true),
                    recetorTran = table.Column<int>(type: "int", nullable: true),
                    pagaTran = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Transaca__EA9168CEB369504C", x => x.transacaoID);
                    table.ForeignKey(
                        name: "FKTransacaoE589588",
                        column: x => x.transacaoID,
                        principalTable: "Transacao",
                        principalColumn: "transacaoID");
                });

            migrationBuilder.CreateTable(
                name: "Utilizador",
                columns: table => new
                {
                    utilizadorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nomeUtilizador = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    password = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    numCares = table.Column<int>(type: "int", nullable: true),
                    moradaID = table.Column<int>(type: "int", nullable: false),
                    tipoUtilizadorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Utilizad__F61D8917D5DAE25F", x => x.utilizadorID);
                    table.ForeignKey(
                        name: "FKUtilizador319669",
                        column: x => x.tipoUtilizadorID,
                        principalTable: "TipoUtilizador",
                        principalColumn: "tipoUtilizadorID");
                    table.ForeignKey(
                        name: "FKUtilizador388471",
                        column: x => x.moradaID,
                        principalTable: "Morada",
                        principalColumn: "moradaID");
                });

            migrationBuilder.CreateTable(
                name: "Emprestimo",
                columns: table => new
                {
                    emprestimoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dataIni = table.Column<DateTime>(type: "datetime", nullable: true),
                    dataDev = table.Column<DateTime>(type: "datetime", nullable: true),
                    transacaoID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Empresti__D470E7830BFF4320", x => x.emprestimoID);
                    table.ForeignKey(
                        name: "FKEmprestimo891193",
                        column: x => x.transacaoID,
                        principalTable: "TransacaoEmprestimos",
                        principalColumn: "transacaoID");
                });

            migrationBuilder.CreateTable(
                name: "Chat",
                columns: table => new
                {
                    chatID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    utilizadorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Chat__8263854DE07F2A4F", x => x.chatID);
                    table.ForeignKey(
                        name: "FKChat902984",
                        column: x => x.utilizadorID,
                        principalTable: "Utilizador",
                        principalColumn: "utilizadorID");
                });

            migrationBuilder.CreateTable(
                name: "Contacto",
                columns: table => new
                {
                    contactoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    numContacto = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    utilizadorID = table.Column<int>(type: "int", nullable: false),
                    tipoContactoID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Contacto__0ECCADC7FB690AAD", x => x.contactoID);
                    table.ForeignKey(
                        name: "FKContacto906402",
                        column: x => x.tipoContactoID,
                        principalTable: "TipoContacto",
                        principalColumn: "tipoContactoID");
                    table.ForeignKey(
                        name: "FKContacto995974",
                        column: x => x.utilizadorID,
                        principalTable: "Utilizador",
                        principalColumn: "utilizadorID");
                });

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

            migrationBuilder.CreateTable(
                name: "PedidosAjuda",
                columns: table => new
                {
                    pedidoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    descPedido = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    recompensaCares = table.Column<int>(type: "int", nullable: true),
                    estado = table.Column<int>(type: "int", nullable: false),
                    horarioAjuda = table.Column<DateTime>(type: "datetime", nullable: true),
                    nHoras = table.Column<int>(type: "int", nullable: true),
                    transacaoID = table.Column<int>(type: "int", nullable: false),
                    nPessoas = table.Column<int>(type: "int", nullable: true),
                    utilizadorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PedidoAj__BAF07AE4789D03F6", x => x.pedidoID);
                    table.ForeignKey(
                        name: "FKPedidoAjud452367",
                        column: x => x.utilizadorID,
                        principalTable: "Utilizador",
                        principalColumn: "utilizadorID");
                    table.ForeignKey(
                        name: "FKPedidoAjud864110",
                        column: x => x.transacaoID,
                        principalTable: "TransacaoAjuda",
                        principalColumn: "transacaoID");
                });

            migrationBuilder.CreateTable(
                name: "Venda",
                columns: table => new
                {
                    transacaoID = table.Column<int>(type: "int", nullable: false),
                    nArtigos = table.Column<int>(type: "int", nullable: true),
                    utilizadorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Venda__EA9168CECCA8D27D", x => x.transacaoID);
                    table.ForeignKey(
                        name: "FKVenda344605",
                        column: x => x.utilizadorID,
                        principalTable: "Utilizador",
                        principalColumn: "utilizadorID");
                    table.ForeignKey(
                        name: "FKVenda552087",
                        column: x => x.transacaoID,
                        principalTable: "Transacao",
                        principalColumn: "transacaoID");
                });

            migrationBuilder.CreateTable(
                name: "Emprestimo_ItemEmprestimo",
                columns: table => new
                {
                    itemID = table.Column<int>(type: "int", nullable: false),
                    emprestimoID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Empresti__7BE62632FBE4295A", x => new { x.itemID, x.emprestimoID });
                    table.ForeignKey(
                        name: "FKEmprestimo135708",
                        column: x => x.emprestimoID,
                        principalTable: "Emprestimo",
                        principalColumn: "emprestimoID");
                    table.ForeignKey(
                        name: "FKEmprestimo851433",
                        column: x => x.itemID,
                        principalTable: "ItemEmprestimo",
                        principalColumn: "itemID");
                });

            migrationBuilder.CreateTable(
                name: "Mensagem",
                columns: table => new
                {
                    mensagemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    conteudo = table.Column<int>(type: "int", nullable: true),
                    dataEnvio = table.Column<DateTime>(type: "datetime", nullable: true),
                    chatID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Mensagem__8419FF1A3FB0425E", x => x.mensagemID);
                    table.ForeignKey(
                        name: "FKMensagem208498",
                        column: x => x.chatID,
                        principalTable: "Chat",
                        principalColumn: "chatID");
                });

            migrationBuilder.CreateTable(
                name: "Notificacao",
                columns: table => new
                {
                    notificacaoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    mensagem = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    lida = table.Column<byte>(type: "tinyint", nullable: true),
                    dataMensagem = table.Column<DateTime>(type: "datetime", nullable: true),
                    pedidoID = table.Column<int>(type: "int", nullable: false),
                    utilizadorID = table.Column<int>(type: "int", nullable: false),
                    itemID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__A917F5716C707642", x => x.notificacaoID);
                    table.ForeignKey(
                        name: "FKNotificaca469461",
                        column: x => x.pedidoID,
                        principalTable: "PedidosAjuda",
                        principalColumn: "pedidoID");
                    table.ForeignKey(
                        name: "FKNotificaca657866",
                        column: x => x.itemID,
                        principalTable: "ItemEmprestimo",
                        principalColumn: "itemID");
                    table.ForeignKey(
                        name: "FKNotificaca99191",
                        column: x => x.utilizadorID,
                        principalTable: "Utilizador",
                        principalColumn: "utilizadorID");
                });

            migrationBuilder.CreateTable(
                name: "Voluntariado",
                columns: table => new
                {
                    pedidoID = table.Column<int>(type: "int", nullable: false),
                    utilizadorID = table.Column<int>(type: "int", nullable: false),
                    idVoluntariado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Voluntar__A4B38AAC16F79537", x => new { x.pedidoID, x.utilizadorID, x.idVoluntariado });
                    table.ForeignKey(
                        name: "FKVoluntaria135401",
                        column: x => x.utilizadorID,
                        principalTable: "Utilizador",
                        principalColumn: "utilizadorID");
                    table.ForeignKey(
                        name: "FKVoluntaria267537",
                        column: x => x.pedidoID,
                        principalTable: "PedidosAjuda",
                        principalColumn: "pedidoID");
                });

            migrationBuilder.CreateTable(
                name: "Artigo",
                columns: table => new
                {
                    artigoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nomeArtigo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    descArtigo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    custoCares = table.Column<int>(type: "int", nullable: true),
                    lojaID = table.Column<int>(type: "int", nullable: false),
                    transacaoID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Artigo__34661042A75121D9", x => x.artigoID);
                    table.ForeignKey(
                        name: "FKArtigo399404",
                        column: x => x.lojaID,
                        principalTable: "Loja",
                        principalColumn: "lojaID");
                    table.ForeignKey(
                        name: "FKArtigo680902",
                        column: x => x.transacaoID,
                        principalTable: "Venda",
                        principalColumn: "transacaoID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artigo_lojaID",
                table: "Artigo",
                column: "lojaID");

            migrationBuilder.CreateIndex(
                name: "IX_Artigo_transacaoID",
                table: "Artigo",
                column: "transacaoID");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_utilizadorID",
                table: "Chat",
                column: "utilizadorID");

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_tipoContactoID",
                table: "Contacto",
                column: "tipoContactoID");

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_utilizadorID",
                table: "Contacto",
                column: "utilizadorID");

            migrationBuilder.CreateIndex(
                name: "IX_Emprestimo_transacaoID",
                table: "Emprestimo",
                column: "transacaoID");

            migrationBuilder.CreateIndex(
                name: "IX_Emprestimo_ItemEmprestimo_emprestimoID",
                table: "Emprestimo_ItemEmprestimo",
                column: "emprestimoID");

            migrationBuilder.CreateIndex(
                name: "IX_ItemEmprestimo_Utilizador_utilizadorID",
                table: "ItemEmprestimo_Utilizador",
                column: "utilizadorID");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagem_chatID",
                table: "Mensagem",
                column: "chatID");

            migrationBuilder.CreateIndex(
                name: "IX_Morada_CPID",
                table: "Morada",
                column: "CPID");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacao_itemID",
                table: "Notificacao",
                column: "itemID");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacao_pedidoID",
                table: "Notificacao",
                column: "pedidoID");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacao_utilizadorID",
                table: "Notificacao",
                column: "utilizadorID");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosAjuda_transacaoID",
                table: "PedidosAjuda",
                column: "transacaoID");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosAjuda_utilizadorID",
                table: "PedidosAjuda",
                column: "utilizadorID");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizador_moradaID",
                table: "Utilizador",
                column: "moradaID");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizador_tipoUtilizadorID",
                table: "Utilizador",
                column: "tipoUtilizadorID");

            migrationBuilder.CreateIndex(
                name: "IX_Venda_utilizadorID",
                table: "Venda",
                column: "utilizadorID");

            migrationBuilder.CreateIndex(
                name: "IX_Voluntariado_utilizadorID",
                table: "Voluntariado",
                column: "utilizadorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Artigo");

            migrationBuilder.DropTable(
                name: "Contacto");

            migrationBuilder.DropTable(
                name: "Emprestimo_ItemEmprestimo");

            migrationBuilder.DropTable(
                name: "ItemEmprestimo_Utilizador");

            migrationBuilder.DropTable(
                name: "Mensagem");

            migrationBuilder.DropTable(
                name: "Notificacao");

            migrationBuilder.DropTable(
                name: "Voluntariado");

            migrationBuilder.DropTable(
                name: "Loja");

            migrationBuilder.DropTable(
                name: "Venda");

            migrationBuilder.DropTable(
                name: "TipoContacto");

            migrationBuilder.DropTable(
                name: "Emprestimo");

            migrationBuilder.DropTable(
                name: "Chat");

            migrationBuilder.DropTable(
                name: "ItemEmprestimo");

            migrationBuilder.DropTable(
                name: "PedidosAjuda");

            migrationBuilder.DropTable(
                name: "TransacaoEmprestimos");

            migrationBuilder.DropTable(
                name: "Utilizador");

            migrationBuilder.DropTable(
                name: "TransacaoAjuda");

            migrationBuilder.DropTable(
                name: "TipoUtilizador");

            migrationBuilder.DropTable(
                name: "Morada");

            migrationBuilder.DropTable(
                name: "Transacao");

            migrationBuilder.DropTable(
                name: "CP");
        }
    }
}
