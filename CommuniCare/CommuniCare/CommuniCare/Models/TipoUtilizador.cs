using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class TipoUtilizador
{
    public int TipoUtilizadorId { get; set; }

    public string? DescTU { get; set; }

    public virtual ICollection<Utilizador> Utilizadors { get; set; } = new List<Utilizador>();
}
