/// <summary>
/// Namespace que contém todas as classes do modelo de dados da aplicação CommuniCare.
/// </summary>
/// <remarks>
/// Este namespace é utilizado para agrupar todas as classes de modelo que representam as entidades e as suas relações dentro da aplicação CommuniCare.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa uma transação de ajuda, associando uma transação a um ou mais pedidos de ajuda.
    /// </summary>
    /// <remarks>
    /// Esta classe é usada para modelar a associação entre uma transação de ajuda e os pedidos de ajuda que fazem parte da transação. 
    /// Cada instância de <see cref="TransacaoAjuda"/> pode ter múltiplos pedidos de ajuda associados a ela e é vinculada a uma transação financeira ou de outro tipo.
    /// </remarks>
    public partial class TransacaoAjuda
    {

        #region Atributos

        /// <summary>
        /// ID do recetor da transação de ajuda. Pode ser nulo, indicando que a transação não foi atribuída a um recetor específico.
        /// </summary>
        int? recetorTran;

        /// <summary>
        /// ID único da transação de ajuda.
        /// </summary>
        int transacaoId;

        /// <summary>
        /// Coleção de pedidos de ajuda associados a esta transação.
        /// </summary>
        ICollection<PedidoAjuda> pedidoAjuda = new List<PedidoAjuda>();
        
        /// <summary>
        /// A transação associada a este objeto de ajuda.
        /// </summary>
        Transacao transacao = null!;

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o ID do recetor da transação de ajuda.
        /// </summary>
        public int? RecetorTran
        {
            get { return recetorTran; }
            set { recetorTran = value; }
        }

        /// <summary>
        /// Obtém ou define o ID da transação de ajuda.
        /// </summary>
        public int TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de pedidos de ajuda associados a esta transação de ajuda.
        /// </summary>
        public virtual ICollection<PedidoAjuda> PedidoAjuda
        {
            get { return pedidoAjuda; }
            set { pedidoAjuda = value; }
        }

        /// <summary>
        /// Obtém ou define a transação associada a este objeto de ajuda.
        /// </summary>
        public virtual Transacao Transacao
        {
            get { return transacao; }
            set { transacao = value; }
        }

        #endregion

        #region Construtores

        #endregion
   
    }
}
