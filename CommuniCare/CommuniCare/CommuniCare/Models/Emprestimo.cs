using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class Emprestimo
    {
        
        #region Atributos

        int emprestimoId;

        DateTime? dataIni;

        DateTime? dataDev;

        int? transacaoId;

        TransacaoEmprestimo transacao = null!;

        ICollection<ItemEmprestimo> items = new List<ItemEmprestimo>();
        
        ICollection<ItemEmprestimoUtilizador> itemEmprestimoUtilizadores = new List<ItemEmprestimoUtilizador>();

        #endregion

        #region Propriedades

        public int EmprestimoId
        {
            get { return emprestimoId; }
            set { emprestimoId = value; }
        }

        public DateTime? DataIni
        {
            get { return dataIni; }
            set { dataIni = value; }
        }

        public DateTime? DataDev
        {
            get { return dataDev; }
            set { dataDev = value; }
        }

        public int? TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        public virtual TransacaoEmprestimo Transacao
        {
            get { return transacao; }
            set { transacao = value; }
        }

        public virtual ICollection<ItemEmprestimo> Items
        {
            get { return items; }
            set { items = value; }
        }

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