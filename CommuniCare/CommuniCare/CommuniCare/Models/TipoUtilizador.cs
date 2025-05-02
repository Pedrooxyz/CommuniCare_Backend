/// <summary>
/// Representa um tipo de utilizador no sistema CommuniCare.
/// </summary>
/// <remarks>
/// A classe <see cref="TipoUtilizador"/> define os diferentes tipos de utilizadores dentro do sistema. 
/// Cada tipo de utilizador é associado a uma coleção de utilizadores através da propriedade <see cref="Utilizadors"/>. 
/// A descrição do tipo de utilizador pode ser definida através da propriedade <see cref="DescTU"/>.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa o tipo de utilizador, incluindo a descrição e os utilizadores associados.
    /// </summary>
    public partial class TipoUtilizador
    {

        #region Atributos

        /// <summary>
        /// Identificador único do tipo de utilizador.
        /// </summary>
        int tipoUtilizadorId;

        /// <summary>
        /// Descrição do tipo de utilizador.
        /// </summary>
        string? descTU;

        /// <summary>
        /// Coleção de utilizadores associados a este tipo de utilizador.
        /// </summary>
        ICollection<Utilizador> utilizadors = new List<Utilizador>();

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único do tipo de utilizador.
        /// </summary>
        public int TipoUtilizadorId
        {
            get { return tipoUtilizadorId; }
            set { tipoUtilizadorId = value; }
        }

        /// <summary>
        /// Obtém ou define a descrição do tipo de utilizador.
        /// </summary>
        public string? DescTU
        {
            get { return descTU; }
            set { descTU = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de utilizadores associados a este tipo de utilizador.
        /// </summary>
        public virtual ICollection<Utilizador> Utilizadors
        {
            get { return utilizadors; }
            set { utilizadors = value; }
        }

        #endregion

        #region Construtores

        #endregion
    
    }
}
