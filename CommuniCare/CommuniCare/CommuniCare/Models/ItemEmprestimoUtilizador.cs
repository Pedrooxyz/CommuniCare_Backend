/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo as entidades relacionadas com a associação entre 
/// itens de empréstimo e utilizadores, como a classe <see cref="ItemEmprestimoUtilizador"/>.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="ItemEmprestimoUtilizador"/> representa a associação entre um item de empréstimo, um utilizador e um empréstimo,
/// incluindo informações sobre o tipo de relação (como "emprestador" ou "utilizador"), o item e o empréstimo aos quais o utilizador está associado.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa a associação entre um item de empréstimo e um utilizador.
    /// </summary>
    public class ItemEmprestimoUtilizador
    {

        #region Atributos

        /// <summary>
        /// Identificador único da associação.
        /// </summary>
        int itemEmpId;

        /// <summary>
        /// Identificador do item de empréstimo.
        /// </summary>
        int itemId;

        /// <summary>
        /// Identificador do utilizador associado ao item de empréstimo.
        /// </summary>
        int? utilizadorId;

        /// <summary>
        /// Tipo de relação do utilizador com o item (por exemplo, "emprestador", "utilizador").
        /// </summary>
        string tipoRelacao = null!;

        /// <summary>
        /// Identificador do empréstimo associado ao item.
        /// </summary>
        int? emprestimoId;

        /// <summary>
        /// Empréstimo relacionado com a associação.
        /// </summary>
        Emprestimo emprestimo = null!;

        /// <summary>
        /// Item de empréstimo relacionado com a associação.
        /// </summary>
        ItemEmprestimo itemEmprestimo = null!;

        /// <summary>
        /// Utilizador relacionado com o item de empréstimo.
        /// </summary>
        Utilizador utilizador = null!;

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único da associação.
        /// </summary>
        public int ItemEmpId
        {
            get { return itemEmpId; }
            set { itemEmpId = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do item de empréstimo.
        /// </summary>
        public int ItemId
        {
            get { return itemId; }
            set { itemId = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do utilizador associado ao item de empréstimo.
        /// </summary>
        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        /// <summary>
        /// Obtém ou define o tipo de relação do utilizador com o item.
        /// </summary>
        public string TipoRelacao
        {
            get { return tipoRelacao; }
            set { tipoRelacao = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do empréstimo relacionado com a associação.
        /// </summary>
        public int? EmprestimoId
        {
            get { return emprestimoId; }
            set { emprestimoId = value; }
        }

        /// <summary>
        /// Obtém ou define o empréstimo relacionado com o item de empréstimo.
        /// </summary>
        public virtual Emprestimo Emprestimo
        {
            get { return emprestimo; }
            set { emprestimo = value; }
        }

        /// <summary>
        /// Obtém ou define o item de empréstimo relacionado com a associação.
        /// </summary>
        public virtual ItemEmprestimo ItemEmprestimo
        {
            get { return itemEmprestimo; }
            set { itemEmprestimo = value; }
        }

        /// <summary>
        /// Obtém ou define o utilizador associado ao item de empréstimo.
        /// </summary>
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