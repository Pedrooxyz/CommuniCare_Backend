using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class Cp
    {
        #region Atributos

        string cPostal = null!;

        string? localidade;
        
        ICollection<Morada> morada = new List<Morada>();

        #endregion

        #region Propriedades

        public string CPostal
        {
            get { return cPostal; }
            set { cPostal = value; }
        }

        public string? Localidade
        {
            get { return localidade; }
            set { localidade = value; }
        }

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