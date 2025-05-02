using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class TipoContacto
    {

        #region Atributos

        int tipoContactoId;

        string? descContacto;

        ICollection<Contacto> contactos = new List<Contacto>();

        #endregion

        #region Propriedades

        public int TipoContactoId
        {
            get { return tipoContactoId; }
            set { tipoContactoId = value; }
        }

        public string? DescContacto
        {
            get { return descContacto; }
            set { descContacto = value; }
        }

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