/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo as entidades relacionadas com os itens de empréstimo.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="ItemEmprestimo"/> representa um item que pode ser emprestado, com detalhes como nome, descrição, disponibilidade, 
/// comissão em "Cares", entre outros. Esta classe também gerencia os empréstimos, notificações e os utilizadores relacionados com os itens emprestados.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{

    public enum EstadoItemEmprestimo
    {
        Indisponivel = 0,
        Disponivel = 1,
        IndisponivelPermanentemente = 2
    }

    /// <summary>
    /// Representa um item disponível para empréstimo.
    /// </summary>
    public partial class ItemEmprestimo
    {
        
        #region Atributos

        /// <summary>
        /// Identificador único do item.
        /// </summary>
        int itemId;

        /// <summary>
        /// Nome do item.
        /// </summary>
        string? nomeItem;

        /// <summary>
        /// Descrição do item.
        /// </summary>
        string? descItem;

        /// <summary>
        /// Indica se o item está disponível para empréstimo.
        /// </summary>
        EstadoItemEmprestimo? disponivel = EstadoItemEmprestimo.Indisponivel;

        /// <summary>
        /// Fotografia do item.
        /// </summary>
        string? fotografiaItem;

        /// <summary>
        /// Comissão do item em unidades de "Cares".
        /// </summary>
        int? comissaoCares;

        /// <summary>
        /// Identificador do emprestador do item.
        /// </summary>
        int idEmprestador;

        /// <summary>
        /// Lista de notificações relacionadas ao item.
        /// </summary>
        ICollection<Notificacao> notificacaos = new List<Notificacao>();
        
        /// <summary>
        /// Lista de empréstimos associados ao item.
        /// </summary>
        ICollection<Emprestimo> emprestimos = new List<Emprestimo>();
        
        /// <summary>
        /// Lista de utilizadores relacionados ao item.
        /// </summary>
        ICollection<Utilizador> utilizadores = new List<Utilizador>();
        
        /// <summary>
        /// Lista de associações de item com utilizadores nos empréstimos.
        /// </summary>
        ICollection<ItemEmprestimoUtilizador> itemEmprestimoUtilizadores = new List<ItemEmprestimoUtilizador>();

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único do item.
        /// </summary>
        public int ItemId
        {
            get { return itemId; }
            set { itemId = value; }
        }

        /// <summary>
        /// Obtém ou define o nome do item.
        /// </summary>
        public string? NomeItem
        {
            get { return nomeItem; }
            set { nomeItem = value; }
        }

        /// <summary>
        /// Obtém ou define a descrição do item.
        /// </summary>
        public string? DescItem
        {
            get { return descItem; }
            set { descItem = value; }
        }

        /// <summary>
        /// Obtém ou define a disponibilidade do item para empréstimo.
        /// </summary>
        public EstadoItemEmprestimo? Disponivel
        {
            get { return disponivel; }
            set { disponivel = value; }
        }

        /// <summary>
        /// Obtém ou define a fotografia do item.
        /// </summary>
        public string? FotografiaItem
        {
            get { return fotografiaItem; }
            set { fotografiaItem = value; }
        }

        /// <summary>
        /// Obtém ou define a comissão do item em unidades de "Cares".
        /// </summary>
        public int? ComissaoCares
        {
            get { return comissaoCares; }
            set { comissaoCares = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do emprestador do item.
        /// </summary>
        public int IdEmprestador
        {
            get { return idEmprestador; }
            set { idEmprestador = value; }
        }

        /// <summary>
        /// Obtém ou define a lista de notificações associadas ao item.
        /// </summary>
        public virtual ICollection<Notificacao> Notificacaos
        {
            get { return notificacaos; }
            set { notificacaos = value; }
        }

        /// <summary>
        /// Obtém ou define a lista de empréstimos associados ao item.
        /// </summary>
        public virtual ICollection<Emprestimo> Emprestimos
        {
            get { return emprestimos; }
            set { emprestimos = value; }
        }

        /// <summary>
        /// Obtém ou define a lista de utilizadores relacionados com o item.
        /// </summary>
        public virtual ICollection<Utilizador> Utilizadores
        {
            get { return utilizadores; }
            set { utilizadores = value; }
        }

        /// <summary>
        /// Obtém ou define a lista de associações de item com utilizadores nos empréstimos.
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
