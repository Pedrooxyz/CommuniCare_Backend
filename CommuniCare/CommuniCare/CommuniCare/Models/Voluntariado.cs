/// <summary>
/// Representa o estado de um voluntariado relacionado com um pedido de ajuda.
/// O voluntariado está associado a um utilizador que oferece ajuda para um pedido específico.
/// </summary>
using System;
using System.Collections.Generic;

namespace CommuniCare.Models{
    
    /// <summary>
    /// Enumerado que representa os possíveis estados de um voluntariado.
    /// </summary>
    public enum EstadoVoluntariado
    {
        /// <summary>
        /// Estado inicial do voluntariado, indicando que o pedido está pendente.
        /// </summary>
        Pendente = 1,

        /// <summary>
        /// Estado indicando que o voluntariado foi aceite.
        /// </summary>
        Aceite = 2
    }

    /// <summary>
    /// Representa o voluntariado de um utilizador associado a um pedido de ajuda.
    /// Contém informações sobre o pedido de ajuda, o utilizador que se voluntariou e o estado do voluntariado.
    /// </summary>
    public partial class Voluntariado
    {
        
        #region Atributos

        /// <summary>
        /// ID do pedido de ajuda associado ao voluntariado.
        /// </summary>
        int pedidoId;

        /// <summary>
        /// ID do utilizador que se voluntariou.
        /// </summary>
        int? utilizadorId;

        /// <summary>
        /// ID único do voluntariado.
        /// </summary>
        int idVoluntariado;

        /// <summary>
        /// Estado do voluntariado, que pode ser "Pendente" ou "Aceite".
        /// </summary>
        EstadoVoluntariado estado = EstadoVoluntariado.Pendente;
        
        /// <summary>
        /// Pedido de ajuda associado ao voluntariado.
        /// </summary>
        PedidoAjuda pedido = null!;
        
        /// <summary>
        /// Utilizador que se voluntariou para o pedido de ajuda.
        /// </summary>
        Utilizador utilizador = null!;

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o ID do pedido de ajuda associado ao voluntariado.
        /// </summary>
        public int PedidoId
        {
            get { return pedidoId; }
            set { pedidoId = value; }
        }

        /// <summary>
        /// Obtém ou define o ID do utilizador que se voluntariou.
        /// </summary>
        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        /// <summary>
        /// Obtém ou define o ID único do voluntariado.
        /// </summary>
        public int IdVoluntariado
        {
            get { return idVoluntariado; }
            set { idVoluntariado = value; }
        }

        /// <summary>
        /// Obtém ou define o estado do voluntariado, que pode ser "Pendente" ou "Aceite".
        /// </summary>
        public EstadoVoluntariado Estado
        {
            get { return estado; }
            set { estado = value; }
        }

        /// <summary>
        /// Obtém ou define o pedido de ajuda associado ao voluntariado.
        /// </summary>
        public virtual PedidoAjuda Pedido
        {
            get { return pedido; }
            set { pedido = value; }
        }

        /// <summary>
        /// Obtém ou define o utilizador que se voluntariou para o pedido de ajuda.
        /// </summary>
        public virtual Utilizador Utilizador
        {
            get { return utilizador; }
            set { utilizador = value; }
        }

        #endregion
    
    }
}