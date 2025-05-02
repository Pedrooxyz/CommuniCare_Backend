/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo as entidades relacionadas com empréstimos,
/// transações de empréstimos e itens emprestados aos utilizadores no sistema.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="Emprestimo"/> representa um empréstimo de itens, contendo informações sobre a data inicial e de devolução,
/// a transação associada ao empréstimo, e a lista de itens emprestados. A classe também mantém o relacionamento com os utilizadores
/// que têm os itens emprestados.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa um empréstimo de itens no sistema.
    /// </summary>
    public partial class Emprestimo
    {
        
        #region Atributos

        /// <summary>
        /// Identificador único do empréstimo.
        /// </summary>
        int emprestimoId;

        /// <summary>
        /// Data inicial do empréstimo.
        /// </summary>
        DateTime? dataIni;

        /// <summary>
        /// Data de devolução do empréstimo.
        /// </summary>
        DateTime? dataDev;

        /// <summary>
        /// Identificador da transação associada ao empréstimo.
        /// </summary>
        int? transacaoId;

        /// <summary>
        /// Transação associada ao empréstimo.
        /// </summary>
        TransacaoEmprestimo transacao = null!;

        /// <summary>
        /// Lista de itens emprestados.
        /// </summary>
        ICollection<ItemEmprestimo> items = new List<ItemEmprestimo>();
         
        /// <summary>
        /// Lista de itens emprestados a utilizadores específicos.
        /// </summary>
        ICollection<ItemEmprestimoUtilizador> itemEmprestimoUtilizadores = new List<ItemEmprestimoUtilizador>();

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único do empréstimo.
        /// </summary>
        public int EmprestimoId
        {
            get { return emprestimoId; }
            set { emprestimoId = value; }
        }

        /// <summary>
        /// Obtém ou define a data inicial do empréstimo.
        /// </summary>
        public DateTime? DataIni
        {
            get { return dataIni; }
            set { dataIni = value; }
        }

        /// <summary>
        /// Obtém ou define a data de devolução do empréstimo.
        /// </summary>
        public DateTime? DataDev
        {
            get { return dataDev; }
            set { dataDev = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador da transação associada ao empréstimo.
        /// </summary>
        public int? TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        /// <summary>
        /// Obtém ou define a transação associada ao empréstimo.
        /// </summary>
        public virtual TransacaoEmprestimo Transacao
        {
            get { return transacao; }
            set { transacao = value; }
        }

        
        /// <summary>
        /// Obtém ou define a lista de itens emprestados.
        /// </summary>
        public virtual ICollection<ItemEmprestimo> Items
        {
            get { return items; }
            set { items = value; }
        }

        /// <summary>
        /// Obtém ou define a lista de itens emprestados a utilizadores específicos.
        /// </summary>
        public virtual ICollection<ItemEmprestimoUtilizador> ItemEmprestimoUtilizadores
        {
            get { return itemEmprestimoUtilizadores; }
            set { itemEmprestimoUtilizadores = value; }
        }

        #endregion

        #region Construtores

        #endregion
    
    }
}