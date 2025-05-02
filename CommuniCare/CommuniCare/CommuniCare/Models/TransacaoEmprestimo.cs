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
    /// Representa uma transação de empréstimo, associando empréstimos e transações financeiras.
    /// </summary>
    /// <remarks>
    /// Esta classe modela a associação entre um empréstimo e uma transação financeira. 
    /// Cada instância de <see cref="TransacaoEmprestimo"/> pode ter múltiplos empréstimos associados a ela, 
    /// bem como detalhes sobre as transações financeiras relacionadas, como horas e valores pagos.
    /// </remarks>
    public partial class TransacaoEmprestimo
    {

        #region Atributos

        /// <summary>
        /// Número de horas associadas à transação de empréstimo.
        /// </summary>
        int? nHoras;

        /// <summary>
        /// ID do recetor da transação de empréstimo.
        /// </summary>
        int? recetorTran;

        /// <summary>
        /// ID do pagador da transação de empréstimo.
        /// </summary>
        int? pagaTran;

        /// <summary>
        /// ID único da transação de empréstimo.
        /// </summary>
        int transacaoId;

        /// <summary>
        /// Coleção de empréstimos associados a esta transação de empréstimo.
        /// </summary>
        ICollection<Emprestimo> emprestimos = new List<Emprestimo>();
        
        /// <summary>
        /// A transação associada a este empréstimo.
        /// </summary>
        Transacao transacao = null!;

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o número de horas associadas à transação de empréstimo.
        /// </summary>
        public int? NHoras
        {
            get { return nHoras; }
            set { nHoras = value; }
        }

        /// <summary>
        /// Obtém ou define o ID do recetor da transação de empréstimo.
        /// </summary>
        public int? RecetorTran
        {
            get { return recetorTran; }
            set { recetorTran = value; }
        }

        /// <summary>
        /// Obtém ou define o ID do pagador da transação de empréstimo.
        /// </summary>
        public int? PagaTran
        {
            get { return pagaTran; }
            set { pagaTran = value; }
        }

        /// <summary>
        /// Obtém ou define o ID da transação de empréstimo.
        /// </summary>
        public int TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de empréstimos associados a esta transação de empréstimo.
        /// </summary>
        public virtual ICollection<Emprestimo> Emprestimos
        {
            get { return emprestimos; }
            set { emprestimos = value; }
        }

        /// <summary>
        /// Obtém ou define a transação associada a este empréstimo.
        /// </summary>
        public virtual Transacao Transacao
        {
            get { return transacao; }
            set { transacao = value; }
        }

        #endregion

    }

}