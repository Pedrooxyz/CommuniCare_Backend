using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public enum EstadoVoluntariado
{
    Pendente = 1,
    Aceite = 2
}

public partial class Voluntariado
{
    public int PedidoId { get; set; }
    public int? UtilizadorId { get; set; }
    public int IdVoluntariado { get; set; }

    public EstadoVoluntariado Estado { get; set; } = EstadoVoluntariado.Pendente;

    public virtual PedidoAjuda Pedido { get; set; } = null!;
    public virtual Utilizador Utilizador { get; set; } = null!;
}
