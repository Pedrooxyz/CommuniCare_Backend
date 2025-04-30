using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Favoritos
{

    public int? UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;

    public int ArtigoId { get; set; }
    public Artigo Artigo { get; set; } = null!;

    public DateTime AddedOn { get; set; } = DateTime.UtcNow;
}

