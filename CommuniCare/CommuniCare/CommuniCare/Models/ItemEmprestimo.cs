using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class ItemEmprestimo
    {
        
        #region Atributos

        int itemId;

        string? nomeItem;

        string? descItem;

        byte? disponivel;

        string? fotografiaItem;

        int? comissaoCares;

        int idEmprestador;

        ICollection<Notificacao> notificacaos = new List<Notificacao>();
        
        ICollection<Emprestimo> emprestimos = new List<Emprestimo>();
        
        ICollection<Utilizador> utilizadores = new List<Utilizador>();
        
        ICollection<ItemEmprestimoUtilizador> itemEmprestimoUtilizadores = new List<ItemEmprestimoUtilizador>();

        #endregion

        #region Propriedades

        public int ItemId
        {
            get { return itemId; }
            set { itemId = value; }
        }

        public string? NomeItem
        {
            get { return nomeItem; }
            set { nomeItem = value; }
        }

        public string? DescItem
        {
            get { return descItem; }
            set { descItem = value; }
        }

        public byte? Disponivel
        {
            get { return disponivel; }
            set { disponivel = value; }
        }

        public string? FotografiaItem
        {
            get { return fotografiaItem; }
            set { fotografiaItem = value; }
        }

        public int? ComissaoCares
        {
            get { return comissaoCares; }
            set { comissaoCares = value; }
        }

        public int IdEmprestador
        {
            get { return idEmprestador; }
            set { idEmprestador = value; }
        }

        public virtual ICollection<Notificacao> Notificacaos
        {
            get { return notificacaos; }
            set { notificacaos = value; }
        }

        public virtual ICollection<Emprestimo> Emprestimos
        {
            get { return emprestimos; }
            set { emprestimos = value; }
        }

        public virtual ICollection<Utilizador> Utilizadores
        {
            get { return utilizadores; }
            set { utilizadores = value; }
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
