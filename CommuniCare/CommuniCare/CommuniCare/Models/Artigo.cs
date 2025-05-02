/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo as entidades que representam
/// artigos, lojas, transações, favoritos e outros componentes necessários para a funcionalidade do sistema.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="Artigo"/> representa um artigo disponível para venda em uma loja no sistema,
/// incluindo propriedades como nome, descrição, custo, quantidade disponível, entre outros. 
/// Ela também se relaciona com outras entidades como <see cref="Loja"/> e <see cref="Venda"/>.
/// O estado do artigo pode ser alterado entre disponível e indisponível, e a classe também gerencia
/// a lista de favoritos de usuários associados ao artigo.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models{

    /// <summary>
    /// Representa os possíveis estados de um artigo.
    /// </summary>
    public enum EstadoArtigo
    {
        /// <summary>
        /// O artigo está disponível.
        /// </summary>
        Disponivel,
        
        /// <summary>
        /// O artigo está indisponível.
        /// </summary>
        Indisponivel
    }

    /// <summary>
    /// Representa um artigo que pode ser adquirido numa loja.
    /// </summary>
    public partial class Artigo
    {

        #region Atributos

        /// <summary>
        /// Identificador único do artigo.
        /// </summary>
        int artigoId;

        /// <summary>
        /// Nome do artigo.
        /// </summary>
        string? nomeArtigo;

        /// <summary>
        /// Descrição do artigo.
        /// </summary>
        string? descArtigo;

        /// <summary>
        /// Custo do artigo em unidades de "Cares".
        /// </summary>
        int? custoCares;

        /// <summary>
        /// Identificador da loja que oferece o artigo.
        /// </summary>
        int lojaId;

        /// <summary>
        /// Identificador da transação associada ao artigo.
        /// </summary>
        int? transacaoId;

        /// <summary>
        /// Quantidade disponível do artigo.
        /// </summary>
        int quantidadeDisponivel;

        /// <summary>
        /// Fotografia do artigo.
        /// </summary>
        string? fotografiaArt;

        /// <summary>
        /// Loja que oferece o artigo.
        /// </summary>
        Loja loja = null!;

        /// <summary>
        /// Transação associada ao artigo, caso exista.
        /// </summary>
        Venda? transacao;

        /// <summary>
        /// Estado atual do artigo.
        /// </summary>
        EstadoArtigo estado = EstadoArtigo.Disponivel;

        /// <summary>
        /// Lista de favoritos associados ao artigo.
        /// </summary>
        ICollection<Favoritos> favoritoPor = new List<Favoritos>();

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único do artigo.
        /// </summary>
        public int ArtigoId
        {
            get { return artigoId; }
            set { artigoId = value; }
        }

        /// <summary>
        /// Obtém ou define o nome do artigo.
        /// </summary>
        public string? NomeArtigo
        {
            get { return nomeArtigo; }
            set { nomeArtigo = value; }
        }

        /// <summary>
        /// Obtém ou define a descrição do artigo.
        /// </summary>
        public string? DescArtigo
        {
            get { return descArtigo; }
            set { descArtigo = value; }
        }

        /// <summary>
        /// Obtém ou define o custo do artigo em unidades de "Cares".
        /// </summary>
        public int? CustoCares
        {
            get { return custoCares; }
            set { custoCares = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador da loja que oferece o artigo.
        /// </summary>
        public int LojaId
        {
            get { return lojaId; }
            set { lojaId = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador da transação associada ao artigo.
        /// </summary>
        public int? TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        /// <summary>
        /// Obtém ou define a quantidade disponível do artigo.
        /// </summary>
        public int QuantidadeDisponivel
        {
            get { return quantidadeDisponivel; }
            set { quantidadeDisponivel = value; }
        }

        /// <summary>
        /// Obtém ou define a fotografia do artigo.
        /// </summary>
        public string? FotografiaArt
        {
            get { return fotografiaArt; }
            set { fotografiaArt = value; }
        }

        /// <summary>
        /// Obtém ou define o estado do artigo.
        /// </summary>
        public EstadoArtigo Estado
        {
            get { return estado; }
            set { estado = value; }
        }

        /// <summary>
        /// Obtém ou define a loja que oferece o artigo.
        /// </summary>
        public virtual Loja Loja
        {
            get { return loja; }
            set { loja = value; }
        }

        /// <summary>
        /// Obtém ou define a transação associada ao artigo.
        /// </summary>
        public virtual Venda? Transacao
        {
            get { return transacao; }
            set { transacao = value; }
        }

        /// <summary>
        /// Obtém ou define a lista de favoritos associados ao artigo.
        /// </summary>
        public virtual ICollection<Favoritos> FavoritoPor
        {
            get { return favoritoPor; }
            set { favoritoPor = value; }
        }

        #endregion

        #region Construtores

        #endregion
        
    }

}