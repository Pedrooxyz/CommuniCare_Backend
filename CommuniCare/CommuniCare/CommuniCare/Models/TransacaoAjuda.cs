using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class TransacaoAjuda
    {

        #region Atributos

        int? recetorTran;

        int transacaoId;

        ICollection<PedidoAjuda> pedidoAjuda = new List<PedidoAjuda>();
        
        Transacao transacao = null!;

        #endregion

        #region Propriedades

        public int? RecetorTran
        {
            get { return recetorTran; }
            set { recetorTran = value; }
        }

        public int TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        public virtual ICollection<PedidoAjuda> PedidoAjuda
        {
            get { return pedidoAjuda; }
            set { pedidoAjuda = value; }
        }

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
