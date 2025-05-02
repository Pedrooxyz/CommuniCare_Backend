using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class Morada
    {

        #region Atributos

        int moradaId;

        string? rua;

        int? numPorta;

        string? cPostal;

        Cp cp = null!;

        ICollection<Utilizador> utilizadores = new List<Utilizador>();

        #endregion

        #region Propriedades

        public int MoradaId
        {
            get { return moradaId; }
            set { moradaId = value; }
        }

        public string? Rua
        {
            get { return rua; }
            set { rua = value; }
        }

        public int? NumPorta
        {
            get { return numPorta; }
            set { numPorta = value; }
        }

        public string? CPostal
        {
            get { return cPostal; }
            set { cPostal = value; }
        }

        public virtual Cp Cp
        {
            get { return cp; }
            set { cp = value; }
        }

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
