using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public class ItemEmprestimoUtilizador
    {

        #region Atributos

        int itemEmpId;

        int itemId;

        int? utilizadorId;

        string tipoRelacao = null!;

        int? emprestimoId;

        Emprestimo emprestimo = null!;

        ItemEmprestimo itemEmprestimo = null!;

        Utilizador utilizador = null!;

        #endregion

        #region Propriedades

        public int ItemEmpId
        {
            get { return itemEmpId; }
            set { itemEmpId = value; }
        }

        public int ItemId
        {
            get { return itemId; }
            set { itemId = value; }
        }

        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        public string TipoRelacao
        {
            get { return tipoRelacao; }
            set { tipoRelacao = value; }
        }

        public int? EmprestimoId
        {
            get { return emprestimoId; }
            set { emprestimoId = value; }
        }

        public virtual Emprestimo Emprestimo
        {
            get { return emprestimo; }
            set { emprestimo = value; }
        }

        public virtual ItemEmprestimo ItemEmprestimo
        {
            get { return itemEmprestimo; }
            set { itemEmprestimo = value; }
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