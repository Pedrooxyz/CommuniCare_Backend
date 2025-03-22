using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Venda
{
    public int? NArtigos { get; set; }

    public int UtilizadorId { get; set; }

    public int TransacaoId { get; set; }

    public virtual ICollection<Artigo> Artigos { get; set; } = new List<Artigo>();

    public virtual Transacao Transacao { get; set; } = null!;

    public virtual Utilizador Utilizador { get; set; } = null!;
}
