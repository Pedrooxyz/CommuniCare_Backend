/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo as entidades relacionadas com os contactos
/// dos utilizadores e os tipos de contacto no sistema.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="Contacto"/> representa um contacto de um utilizador, contendo informações sobre o número de
/// contacto, o tipo de contacto (por exemplo, telefone, email) e a associação ao utilizador e tipo de contacto específicos.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa um contacto de um utilizador no sistema.
    /// </summary>
    public partial class Contacto
    {

        #region Atributos

        /// <summary>
        /// Identificador único do contacto.
        /// </summary>
        int contactoId;

        /// <summary>
        /// Número de contacto (telefone, email, etc.).
        /// </summary>
        string? numContacto;

        /// <summary>
        /// Identificador do utilizador associado ao contacto.
        /// </summary>
        int? utilizadorId;

        /// <summary>
        /// Identificador do tipo de contacto.
        /// </summary>
        int tipoContactoId;

        /// <summary>
        /// Tipo de contacto associado (telefone, email, etc.).
        /// </summary>
        TipoContacto tipoContacto = null!;

        /// <summary>
        /// Utilizador associado ao contacto.
        /// </summary>
        Utilizador utilizador = null!;

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único do contacto.
        /// </summary>
        public int ContactoId
        {
            get { return contactoId; }
            set { contactoId = value; }
        }

        /// <summary>
        /// Obtém ou define o número de contacto (telefone, email, etc.).
        /// </summary>
        public string? NumContacto
        {
            get { return numContacto; }
            set { numContacto = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do utilizador associado ao contacto.
        /// </summary>
        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do tipo de contacto.
        /// </summary>
        public int TipoContactoId
        {
            get { return tipoContactoId; }
            set { tipoContactoId = value; }
        }

        /// <summary>
        /// Obtém ou define o tipo de contacto associado.
        /// </summary>
        public virtual TipoContacto TipoContacto
        {
            get { return tipoContacto; }
            set { tipoContacto = value; }
        }

        /// <summary>
        /// Obtém ou define o utilizador associado ao contacto.
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