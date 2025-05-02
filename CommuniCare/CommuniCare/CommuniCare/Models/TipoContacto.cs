/// <summary>
/// Representa um tipo de contacto no sistema CommuniCare.
/// </summary>
/// <remarks>
/// A classe <see cref="TipoContacto"/> contém a informação sobre os diferentes tipos de contactos que um utilizador pode ter. 
/// Cada tipo de contacto pode estar associado a vários contactos, os quais são definidos pela classe <see cref="Contacto"/>.
/// A propriedade <see cref="DescContacto"/> permite a descrição do tipo de contacto, enquanto a coleção <see cref="Contactos"/>
/// contém todos os contactos associados a este tipo.
/// </remarks>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Representa o tipo de contacto, incluindo a descrição do tipo e a coleção de contactos associados.
    /// </summary>
    public partial class TipoContacto
    {

        #region Atributos

        /// <summary>
        /// Identificador único do tipo de contacto.
        /// </summary>
        int tipoContactoId;

        /// <summary>
        /// Descrição do tipo de contacto.
        /// </summary>
        string? descContacto;

        /// <summary>
        /// Coleção de contactos associados a este tipo de contacto.
        /// </summary>
        ICollection<Contacto> contactos = new List<Contacto>();

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único do tipo de contacto.
        /// </summary>
        public int TipoContactoId
        {
            get { return tipoContactoId; }
            set { tipoContactoId = value; }
        }

        /// <summary>
        /// Obtém ou define a descrição do tipo de contacto.
        /// </summary>
        public string? DescContacto
        {
            get { return descContacto; }
            set { descContacto = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de contactos associados a este tipo de contacto.
        /// </summary>
        public virtual ICollection<Contacto> Contactos
        {
            get { return contactos; }
            set { contactos = value; }
        }

        #endregion

        #region Construtores

        #endregion
    
    }
}