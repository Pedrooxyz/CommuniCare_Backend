using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class TipoUtilizador
    {

        #region Atributos

        int tipoUtilizadorId;

        string? descTU;

        ICollection<Utilizador> utilizadors = new List<Utilizador>();

        #endregion

        #region Propriedades

        public int TipoUtilizadorId
        {
            get { return tipoUtilizadorId; }
            set { tipoUtilizadorId = value; }
        }

        public string? DescTU
        {
            get { return descTU; }
            set { descTU = value; }
        }

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
