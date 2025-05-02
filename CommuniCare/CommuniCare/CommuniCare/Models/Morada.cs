/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo as entidades relacionadas com moradas e utilizadores.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="Morada"/> representa uma morada no sistema, incluindo os dados de rua, número de porta, código postal
/// e a associação com a cidade ou localidade (Cp) e utilizadores.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa uma morada associada a utilizadores e a uma cidade ou localidade no sistema.
    /// </summary>
    public partial class Morada
    {

        #region Atributos

        /// <summary>
        /// Identificador único da morada.
        /// </summary>
        int moradaId;

        /// <summary>
        /// Rua onde a morada está localizada.
        /// </summary>
        string? rua;

        /// <summary>
        /// Número da porta na morada.
        /// </summary>
        int? numPorta;

        /// <summary>
        /// Código postal da morada.
        /// </summary>
        string? cPostal;

        /// <summary>
        /// Cidade ou localidade associada à morada.
        /// </summary>
        Cp cp = null!;

        /// <summary>
        /// Lista de utilizadores associados a esta morada.
        /// </summary>
        ICollection<Utilizador> utilizadores = new List<Utilizador>();

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único da morada.
        /// </summary>
        public int MoradaId
        {
            get { return moradaId; }
            set { moradaId = value; }
        }

        /// <summary>
        /// Obtém ou define o nome da rua da morada.
        /// </summary>
        public string? Rua
        {
            get { return rua; }
            set { rua = value; }
        }

        /// <summary>
        /// Obtém ou define o número da porta na morada.
        /// </summary>
        public int? NumPorta
        {
            get { return numPorta; }
            set { numPorta = value; }
        }

        /// <summary>
        /// Obtém ou define o código postal da morada.
        /// </summary>
        public string? CPostal
        {
            get { return cPostal; }
            set { cPostal = value; }
        }

        /// <summary>
        /// Obtém ou define a cidade ou localidade associada à morada.
        /// </summary>
        public virtual Cp Cp
        {
            get { return cp; }
            set { cp = value; }
        }

        /// <summary>
        /// Obtém ou define a lista de utilizadores associados a esta morada.
        /// </summary>
        public virtual ICollection<Utilizador> Utilizadores
        {
            get { return utilizadores; }
            set { utilizadores = value; }
        }

        #endregion

        #region Construtores

        #endregion
        
    }
}
