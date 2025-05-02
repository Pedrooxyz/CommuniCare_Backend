using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class Contacto
    {

        #region Atributos

        int contactoId;

        string? numContacto;

        int? utilizadorId;

        int tipoContactoId;

        TipoContacto tipoContacto = null!;

        Utilizador utilizador = null!;

        #endregion

        #region Propriedades

        public int ContactoId
        {
            get { return contactoId; }
            set { contactoId = value; }
        }

        public string? NumContacto
        {
            get { return numContacto; }
            set { numContacto = value; }
        }

        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        public int TipoContactoId
        {
            get { return tipoContactoId; }
            set { tipoContactoId = value; }
        }

        public virtual TipoContacto TipoContacto
        {
            get { return tipoContacto; }
            set { tipoContacto = value; }
        }

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