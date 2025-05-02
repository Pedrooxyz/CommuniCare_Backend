/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo as entidades relacionadas com os artigos favoritos dos utilizadores.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="Favoritos"/> representa a associação entre um utilizador e um artigo que foi marcado como favorito. 
/// Ela contém informações sobre o utilizador, o artigo e a data em que o artigo foi adicionado aos favoritos do utilizador.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa a associação entre um utilizador e um artigo favorito.
    /// </summary>
    public partial class Favoritos
    {
        
        #region Atributos

        /// <summary>
        /// Identificador do utilizador que marcou o artigo como favorito.
        /// </summary>
        int? utilizadorId;

        /// <summary>
        /// Utilizador que marcou o artigo como favorito.
        /// </summary>
        Utilizador utilizador = null!;

        /// <summary>
        /// Identificador do artigo que foi marcado como favorito.
        /// </summary>
        int artigoId;

        /// <summary>
        /// Artigo que foi marcado como favorito.
        /// </summary>
        Artigo artigo = null!;

        /// <summary>
        /// Data em que o artigo foi adicionado aos favoritos.
        /// </summary>
        DateTime addedOn = DateTime.UtcNow;

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador do utilizador que marcou o artigo como favorito.
        /// </summary>
        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        /// <summary>
        /// Obtém ou define o utilizador que marcou o artigo como favorito.
        /// </summary>
        public Utilizador Utilizador
        {
            get { return utilizador; }
            set { utilizador = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do artigo que foi marcado como favorito.
        /// </summary>
        public int ArtigoId
        {
            get { return artigoId; }
            set { artigoId = value; }
        }

        /// <summary>
        /// Obtém ou define o artigo que foi marcado como favorito.
        /// </summary>
        public Artigo Artigo
        {
            get { return artigo; }
            set { artigo = value; }
        }

        /// <summary>
        /// Obtém ou define a data em que o artigo foi adicionado aos favoritos.
        /// </summary>
        public DateTime AddedOn
        {
            get { return addedOn; }
            set { addedOn = value; }
        }

        #endregion

        #region Construtores

        #endregion
        
    }
}