using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public enum EstadoLoja
    {
        Inativo = 0,
        Ativo = 1
    }

    public partial class Loja
    {

        #region Atributos

        int lojaId;

        string? nomeLoja;
        
        string? descLoja;

        EstadoLoja estado = EstadoLoja.Ativo;

        ICollection<Artigo> artigos = new List<Artigo>();

        #endregion

        #region Propriedades

        public int LojaId
        {
            get { return lojaId; }
            set { lojaId = value; }
        }

        public string? NomeLoja
        {
            get { return nomeLoja; }
            set { nomeLoja = value; }
        }

        public string? DescLoja
        {
            get { return descLoja; }
            set { descLoja = value; }
        }

        public EstadoLoja Estado
        {
            get { return estado; }
            set { estado = value; }
        }

        public virtual ICollection<Artigo> Artigos
        {
            get { return artigos; }
            set { artigos = value; }
        }

        #endregion

        #region Construtores

        #endregion
    
    }
}
