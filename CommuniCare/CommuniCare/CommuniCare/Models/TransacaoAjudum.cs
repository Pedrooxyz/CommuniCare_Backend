using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class TransacaoAjudum
{
    public int? RecetorTran { get; set; }

    public int TransacaoId { get; set; }

    public virtual ICollection<PedidoAjudum> PedidoAjuda { get; set; } = new List<PedidoAjudum>();

    public virtual Transacao Transacao { get; set; } = null!;
}
