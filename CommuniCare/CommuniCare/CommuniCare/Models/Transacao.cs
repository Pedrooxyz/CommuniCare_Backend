using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class Transacao
    {

        #region Atributos

        int transacaoId;

        DateTime? dataTransacao;

        int? quantidade;

        TransacaoAjuda? transacaoAjuda;

        TransacaoEmprestimo? transacaoEmprestimo;

        Venda? venda;

        #endregion

        #region Propriedades

        public int TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        public DateTime? DataTransacao
        {
            get { return dataTransacao; }
            set { dataTransacao = value; }
        }

        public int? Quantidade
        {
            get { return quantidade; }
            set { quantidade = value; }
        }

        public virtual TransacaoAjuda? TransacaoAjuda
        {
            get { return transacaoAjuda; }
            set { transacaoAjuda = value; }
        }

        public virtual TransacaoEmprestimo? TransacaoEmprestimo
        {
            get { return transacaoEmprestimo; }
            set { transacaoEmprestimo = value; }
        }

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