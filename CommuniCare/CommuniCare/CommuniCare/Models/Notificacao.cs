/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo entidades relacionadas com notificações.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="Notificacao"/> representa uma notificação gerada no sistema, com informações sobre a mensagem,
/// o estado de leitura, a data de envio e a associação com itens, pedidos ou utilizadores.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa uma notificação no sistema, associada a um item, pedido ou utilizador.
    /// </summary>
    public partial class Notificacao
    {

        #region Atributos

        /// <summary>
        /// Identificador único da notificação.
        /// </summary>
        int notificacaoId;

        /// <summary>
        /// Conteúdo da mensagem da notificação.
        /// </summary>
        string? mensagem;

        /// <summary>
        /// Indica se a notificação foi lida (1) ou não (0).
        /// </summary>
        byte? lida;

        /// <summary>
        /// Data em que a mensagem foi enviada.
        /// </summary>
        DateTime? dataMensagem;

        /// <summary>
        /// Identificador do pedido associado à notificação, se houver.
        /// </summary>
        int? pedidoId;

        /// <summary>
        /// Identificador do utilizador associado à notificação, se houver.
        /// </summary>
        int? utilizadorId;

        /// <summary>
        /// Identificador do item associado à notificação, se houver.
        /// </summary>
        int? itemId;

        /// <summary>
        /// Item relacionado com a notificação, se houver.
        /// </summary>
        ItemEmprestimo item = null!;

        /// <summary>
        /// Pedido de ajuda relacionado com a notificação, se houver.
        /// </summary>
        PedidoAjuda pedido = null!;

        /// <summary>
        /// Utilizador relacionado com a notificação, se houver.
        /// </summary>
        Utilizador utilizador = null!;

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único da notificação.
        /// </summary>
        public int NotificacaoId
        {
            get { return notificacaoId; }
            set { notificacaoId = value; }
        }

        /// <summary>
        /// Obtém ou define o conteúdo da mensagem da notificação.
        /// </summary>
        public string? Mensagem
        {
            get { return mensagem; }
            set { mensagem = value; }
        }

        /// <summary>
        /// Obtém ou define o estado de leitura da notificação (0 = não lida, 1 = lida).
        /// </summary>
        public byte? Lida
        {
            get { return lida; }
            set { lida = value; }
        }

        /// <summary>
        /// Obtém ou define a data em que a mensagem foi enviada.
        /// </summary>
        public DateTime? DataMensagem
        {
            get { return dataMensagem; }
            set { dataMensagem = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do pedido associado à notificação.
        /// </summary>
        public int? PedidoId
        {
            get { return pedidoId; }
            set { pedidoId = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do utilizador associado à notificação.
        /// </summary>
        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do item associado à notificação.
        /// </summary>
        public int? ItemId
        {
            get { return itemId; }
            set { itemId = value; }
        }

        /// <summary>
        /// Obtém ou define o item relacionado com a notificação.
        /// </summary>
        public virtual ItemEmprestimo Item
        {
            get { return item; }
            set { item = value; }
        }

        /// <summary>
        /// Obtém ou define o pedido relacionado com a notificação.
        /// </summary>
        public virtual PedidoAjuda Pedido
        {
            get { return pedido; }
            set { pedido = value; }
        }

        /// <summary>
        /// Obtém ou define o utilizador relacionado com a notificação.
        /// </summary>
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
