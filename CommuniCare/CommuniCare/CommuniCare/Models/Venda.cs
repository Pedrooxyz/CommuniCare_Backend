/// <summary>
/// Representa uma venda realizada por um utilizador.
/// A classe contém informações sobre os artigos vendidos, a transação associada e o utilizador que realizou a venda.
/// </summary>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models{

    /// <summary>
    /// Representa uma venda realizada por um utilizador.
    /// A classe contém informações sobre os artigos vendidos, a transação associada e o utilizador que realizou a venda.
    /// </summary>
    public partial class Venda
    {

        #region Atributos

        /// <summary>
        /// Número de artigos envolvidos na venda.
        /// </summary>
        int? nArtigos;

        /// <summary>
        /// ID do utilizador que realizou a venda.
        /// </summary>
        int? utilizadorId;

        /// <summary>
        /// ID da transação associada à venda.
        /// </summary>
        int transacaoId;

        /// <summary>
        /// Coleção de artigos que foram vendidos na transação.
        /// </summary>
        ICollection<Artigo> artigos = new List<Artigo>();
        
        /// <summary>
        /// Transação associada à venda.
        /// </summary>
        Transacao transacao = null!;
        
        /// <summary>
        /// Utilizador que realizou a venda.
        /// </summary>
        Utilizador utilizador = null!;

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o número de artigos envolvidos na venda.
        /// </summary>
        public int? NArtigos
        {
            get { return nArtigos; }
            set { nArtigos = value; }
        }

        /// <summary>
        /// Obtém ou define o ID do utilizador que realizou a venda.
        /// </summary>
        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        /// <summary>
        /// Obtém ou define o ID da transação associada à venda.
        /// </summary>
        public int TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de artigos que foram vendidos.
        /// </summary>
        public virtual ICollection<Artigo> Artigos
        {
            get { return artigos; }
            set { artigos = value; }
        }
        
        /// <summary>
        /// Obtém ou define a transação associada à venda.
        /// </summary>
        public virtual Transacao Transacao
        {
            get { return transacao; }
            set { transacao = value; }
        }

        /// <summary>
        /// Obtém ou define o utilizador que realizou a venda.
        /// </summary>
        public virtual Utilizador Utilizador
        {
            get { return utilizador; }
            set { utilizador = value; }
        }

        #endregion

    }
}
