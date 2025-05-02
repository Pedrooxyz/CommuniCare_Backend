/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo as entidades relacionadas com lojas e artigos.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="Loja"/> representa uma loja no sistema, incluindo informações sobre o nome, descrição, estado e artigos relacionados.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Enumeração que define o estado de uma loja.
    /// </summary>
    public enum EstadoLoja
    {
        /// <summary>
        /// Indica que a loja está inativa.
        /// </summary>
        Inativo = 0,

        /// <summary>
        /// Indica que a loja está ativa.
        /// </summary>
        Ativo = 1
    }
    
    /// <summary>
    /// Representa uma loja no sistema, com informações sobre o nome, descrição, estado e artigos associados.
    /// </summary>
    public partial class Loja
    {

        #region Atributos

        /// <summary>
        /// Identificador único da loja.
        /// </summary>
        int lojaId;

        /// <summary>
        /// Nome da loja.
        /// </summary>
        string? nomeLoja;
        
        /// <summary>
        /// Descrição da loja.
        /// </summary>
        string? descLoja;

        /// <summary>
        /// Estado atual da loja (Ativo ou Inativo).
        /// </summary>
        EstadoLoja estado = EstadoLoja.Ativo;

        /// <summary>
        /// Lista de artigos associados à loja.
        /// </summary>
        ICollection<Artigo> artigos = new List<Artigo>();

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único da loja.
        /// </summary>
        public int LojaId
        {
            get { return lojaId; }
            set { lojaId = value; }
        }

        /// <summary>
        /// Obtém ou define o nome da loja.
        /// </summary>
        public string? NomeLoja
        {
            get { return nomeLoja; }
            set { nomeLoja = value; }
        }

        /// <summary>
        /// Obtém ou define a descrição da loja.
        /// </summary>
        public string? DescLoja
        {
            get { return descLoja; }
            set { descLoja = value; }
        }

        /// <summary>
        /// Obtém ou define o estado da loja (Ativo ou Inativo).
        /// </summary>
        public EstadoLoja Estado
        {
            get { return estado; }
            set { estado = value; }
        }

        /// <summary>
        /// Obtém ou define a lista de artigos associados à loja.
        /// </summary>
        public virtual ICollection<Artigo> Artigos
        {
            get { return artigos; }
            set { artigos = value; }
        }

        #endregion

        #region Construtores

        #endregion
    
    }
}
