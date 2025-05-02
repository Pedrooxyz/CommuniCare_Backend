/// <summary>
/// Representa uma transação no sistema CommuniCare.
/// </summary>
/// <remarks>
/// A classe <see cref="Transacao"/> armazena informações sobre uma transação financeira realizada no sistema. 
/// Pode representar transações relacionadas a ajudas, empréstimos ou vendas. Cada transação inclui uma data, quantidade e referências a outras entidades de transação, 
/// como <see cref="TransacaoAjuda"/>, <see cref="TransacaoEmprestimo"/> ou <see cref="Venda"/>.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa uma transação realizada no sistema, com referências a diferentes tipos de transação.
    /// </summary>
    public partial class Transacao
    {

        #region Atributos

        /// <summary>
        /// Identificador único da transação.
        /// </summary>
        int transacaoId;

        /// <summary>
        /// Data da transação.
        /// </summary>
        DateTime? dataTransacao;

        /// <summary>
        /// Quantidade associada à transação (ex: valor em Cares ou quantidade de itens).
        /// </summary>
        int? quantidade;

        /// <summary>
        /// Transação relacionada a uma ajuda (se houver).
        /// </summary>
        TransacaoAjuda? transacaoAjuda;

        /// <summary>
        /// Transação relacionada a um empréstimo (se houver).
        /// </summary>
        TransacaoEmprestimo? transacaoEmprestimo;

        /// <summary>
        /// Transação relacionada a uma venda (se houver).
        /// </summary>
        Venda? venda;

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único da transação.
        /// </summary>
        public int TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        /// <summary>
        /// Obtém ou define a data da transação.
        /// </summary>
        public DateTime? DataTransacao
        {
            get { return dataTransacao; }
            set { dataTransacao = value; }
        }

        /// <summary>
        /// Obtém ou define a quantidade associada à transação.
        /// </summary>
        public int? Quantidade
        {
            get { return quantidade; }
            set { quantidade = value; }
        }

        /// <summary>
        /// Obtém ou define a transação associada a uma ajuda.
        /// </summary>
        public virtual TransacaoAjuda? TransacaoAjuda
        {
            get { return transacaoAjuda; }
            set { transacaoAjuda = value; }
        }

        /// <summary>
        /// Obtém ou define a transação associada a um empréstimo.
        /// </summary>
        public virtual TransacaoEmprestimo? TransacaoEmprestimo
        {
            get { return transacaoEmprestimo; }
            set { transacaoEmprestimo = value; }
        }

        /// <summary>
        /// Obtém ou define a transação associada a uma venda.
        /// </summary>
        public virtual Venda? Venda
        {
            get { return venda; }
            set { venda = value; }
        }

        #endregion

        #region Construtores

        #endregion
    
    }
}