using System;
using System.Collections.Generic;

namespace CommuniCare.Models{

    public enum EstadoVoluntariado
    {
        Pendente = 1,
        Aceite = 2
    }

    public partial class Voluntariado
    {
        
        #region Atributos

        int pedidoId;

        int? utilizadorId;

        int idVoluntariado;

        EstadoVoluntariado estado = EstadoVoluntariado.Pendente;
        
        PedidoAjuda pedido = null!;
        
        Utilizador utilizador = null!;

        #endregion

        #region Propriedades

        public int PedidoId
        {
            get { return pedidoId; }
            set { pedidoId = value; }
        }

        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        public int IdVoluntariado
        {
            get { return idVoluntariado; }
            set { idVoluntariado = value; }
        }

        public EstadoVoluntariado Estado
        {
            get { return estado; }
            set { estado = value; }
        }

        public virtual PedidoAjuda Pedido
        {
            get { return pedido; }
            set { pedido = value; }
        }

        public virtual Utilizador Utilizador
        {
            get { return utilizador; }
            set { utilizador = value; }
        }

        #endregion
    
    }
}