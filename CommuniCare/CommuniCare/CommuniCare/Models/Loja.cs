using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Loja
{
    public int LojaId { get; set; }

    public string? NomeLoja { get; set; }

    public string? DescLoja { get; set; }

    public virtual ICollection<Artigo> Artigos { get; set; } = new List<Artigo>();
}
