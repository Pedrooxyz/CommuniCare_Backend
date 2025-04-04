using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Morada
{
    public int MoradaId { get; set; }

    public string? Rua { get; set; }

    public int? NumPorta { get; set; }

    public int Cpid { get; set; }

    public virtual Cp Cp { get; set; } = null!;

    public virtual ICollection<Utilizador> Utilizadores { get; set; } = new List<Utilizador>();
}
