using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Notificacao
{
    public int NotificacaoId { get; set; }

    public string? Mensagem { get; set; }

    public byte? Lida { get; set; }

    public DateTime? DataMensagem { get; set; }

    public int? PedidoId { get; set; }

    public int? UtilizadorId { get; set; }

    public int? ItemId { get; set; }

    public virtual ItemEmprestimo Item { get; set; } = null!;

    public virtual PedidoAjuda Pedido { get; set; } = null!;

    public virtual Utilizador Utilizador { get; set; } = null!;
}
