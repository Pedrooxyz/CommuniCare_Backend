using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Voluntariado
{
    public int PedidoId { get; set; }

    public int UtilizadorId { get; set; }

    public int IdVoluntariado { get; set; }

    public virtual PedidoAjudum Pedido { get; set; } = null!;

    public virtual Utilizador Utilizador { get; set; } = null!;
}
