using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Artigo
{
    public int ArtigoId { get; set; }

    public string? NomeArtigo { get; set; }

    public string? DescArtigo { get; set; }

    public int? CustoCares { get; set; }

    public int LojaId { get; set; }

    public int? TransacaoId { get; set; }

    public virtual Loja Loja { get; set; } = null!;

    public virtual Venda? Transacao { get; set; }
}
