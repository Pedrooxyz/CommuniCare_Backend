using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public partial class Favoritos
    {
        
        #region Atributos

        int? utilizadorId;

        Utilizador utilizador = null!;

        int artigoId;

        Artigo artigo = null!;

        DateTime addedOn = DateTime.UtcNow;

        #endregion

        #region Propriedades

        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        public Utilizador Utilizador
        {
            get { return utilizador; }
            set { utilizador = value; }
        }

        public int ArtigoId
        {
            get { return artigoId; }
            set { artigoId = value; }
        }

        public Artigo Artigo
        {
            get { return artigo; }
            set { artigo = value; }
        }

        public DateTime AddedOn
        {
            get { return addedOn; }
            set { addedOn = value; }
        }

        #endregion

        #region Construtores

        #endregion
        
    }
}