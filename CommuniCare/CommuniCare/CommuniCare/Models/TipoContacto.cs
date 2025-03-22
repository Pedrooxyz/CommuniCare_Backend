using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class TipoContacto
{
    public int TipoContactoId { get; set; }

    public string? DescContacto { get; set; }

    public virtual ICollection<Contacto> Contactos { get; set; } = new List<Contacto>();
}
