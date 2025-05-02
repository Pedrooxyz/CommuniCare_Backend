using System;
using System.Collections.Generic;

namespace CommuniCare.Models{

    public partial class TransacaoEmprestimo
    {

        #region Atributos

        int? nHoras;

        int? recetorTran;

        int? pagaTran;

        int transacaoId;

        ICollection<Emprestimo> emprestimos = new List<Emprestimo>();
        
        Transacao transacao = null!;

        #endregion

        #region Propriedades

        public int? NHoras
        {
            get { return nHoras; }
            set { nHoras = value; }
        }

        public int? RecetorTran
        {
            get { return recetorTran; }
            set { recetorTran = value; }
        }

        public int? PagaTran
        {
            get { return pagaTran; }
            set { pagaTran = value; }
        }

        public int TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        public virtual ICollection<Emprestimo> Emprestimos
        {
            get { return emprestimos; }
            set { emprestimos = value; }
        }

        public virtual Transacao Transacao
        {
            get { return transacao; }
            set { transacao = value; }
        }

        #endregion

    }

}