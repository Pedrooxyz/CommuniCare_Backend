using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Contacto
{
    public int ContactoId { get; set; }

    public string? NumContacto { get; set; }

    public int UtilizadorId { get; set; }

    public int TipoContactoId { get; set; }

    public virtual TipoContacto TipoContacto { get; set; } = null!;

    public virtual Utilizador Utilizador { get; set; } = null!;
}
