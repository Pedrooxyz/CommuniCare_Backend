using System;
using System.Collections.Generic;

namespace CommuniCare.Models{


    public enum EstadoArtigo
    {
        Disponivel,
        Indisponivel
    }

    public partial class Artigo
    {

        #region Atributos

        int artigoId;

        string? nomeArtigo;

        string? descArtigo;

        int? custoCares;

        int lojaId;

        int? transacaoId;

        int quantidadeDisponivel;

        string? fotografiaArt;

        Loja loja = null!;

        Venda? transacao;

        EstadoArtigo estado = EstadoArtigo.Disponivel;

        ICollection<Favoritos> favoritoPor = new List<Favoritos>();

        #endregion

        #region Propriedades

        public int ArtigoId
        {
            get { return artigoId; }
            set { artigoId = value; }
        }

        public string? NomeArtigo
        {
            get { return nomeArtigo; }
            set { nomeArtigo = value; }
        }

        public string? DescArtigo
        {
            get { return descArtigo; }
            set { descArtigo = value; }
        }

        public int? CustoCares
        {
            get { return custoCares; }
            set { custoCares = value; }
        }

        public int LojaId
        {
            get { return lojaId; }
            set { lojaId = value; }
        }

        public int? TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        public int QuantidadeDisponivel
        {
            get { return quantidadeDisponivel; }
            set { quantidadeDisponivel = value; }
        }

        public string? FotografiaArt
        {
            get { return fotografiaArt; }
            set { fotografiaArt = value; }
        }

        public EstadoArtigo Estado
        {
            get { return estado; }
            set { estado = value; }
        }

        public virtual Loja Loja
        {
            get { return loja; }
            set { loja = value; }
        }

        public virtual Venda? Transacao
        {
            get { return transacao; }
            set { transacao = value; }
        }

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