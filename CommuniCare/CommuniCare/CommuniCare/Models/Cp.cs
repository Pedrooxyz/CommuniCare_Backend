/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo as entidades relacionadas com os códigos postais,
/// localidades e as moradas associadas no sistema.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="Cp"/> representa um código postal e a sua respetiva localidade, podendo estar associada a várias
/// moradas no sistema. A classe permite organizar e associar diferentes moradas através do código postal e localidade.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa um código postal e a localidade correspondente.
    /// </summary>
    public partial class Cp
    {
        #region Atributos

        /// <summary>
        /// Código postal.
        /// </summary>
        string cPostal = null!;

        /// <summary>
        /// Localidade associada ao código postal.
        /// </summary>
        string? localidade;
        
        /// <summary>
        /// Lista de moradas associadas ao código postal.
        /// </summary>
        ICollection<Morada> morada = new List<Morada>();

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o código postal.
        /// </summary>
        public string CPostal
        {
            get { return cPostal; }
            set { cPostal = value; }
        }

        /// <summary>
        /// Obtém ou define a localidade associada ao código postal.
        /// </summary>
        public string? Localidade
        {
            get { return localidade; }
            set { localidade = value; }
        }

        /// <summary>
        /// Obtém ou define a lista de moradas associadas ao código postal.
        /// </summary>
        public virtual ICollection<Morada> Morada
        {
            get { return morada; }
            set { morada = value; }
        }

        #endregion

        #region Construtores

        #endregion
    }
}