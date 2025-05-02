using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class Notificacao
    {

        #region Atributos

        int notificacaoId;

        string? mensagem;

        byte? lida;

        DateTime? dataMensagem;

        int? pedidoId;

        int? utilizadorId;

        int? itemId;

        ItemEmprestimo item = null!;

        PedidoAjuda pedido = null!;

        Utilizador utilizador = null!;

        #endregion

        #region Propriedades

        public int NotificacaoId
        {
            get { return notificacaoId; }
            set { notificacaoId = value; }
        }

        public string? Mensagem
        {
            get { return mensagem; }
            set { mensagem = value; }
        }

        public byte? Lida
        {
            get { return lida; }
            set { lida = value; }
        }

        public DateTime? DataMensagem
        {
            get { return dataMensagem; }
            set { dataMensagem = value; }
        }

        public int? PedidoId
        {
            get { return pedidoId; }
            set { pedidoId = value; }
        }

        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        public int? ItemId
        {
            get { return itemId; }
            set { itemId = value; }
        }

        public virtual ItemEmprestimo Item
        {
            get { return item; }
            set { item = value; }
        }

        public virtual PedidoAjuda Pedido
        {
            get { return pedido; }
            set { pedido = value; }
        }

        public virtual Utilizador Utilizador
        {
            get { return utilizador; }
            set { utilizador = value; }
        }

        #endregion

        #region Construtores

        #endregion
        
    }
}
