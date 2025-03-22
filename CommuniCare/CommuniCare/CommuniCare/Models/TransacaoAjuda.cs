using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class TransacaoAjuda
{
    public int? RecetorTran { get; set; }

    public int TransacaoId { get; set; }

    public virtual ICollection<PedidoAjuda> PedidoAjuda { get; set; } = new List<PedidoAjuda>();

    public virtual Transacao Transacao { get; set; } = null!;
}
