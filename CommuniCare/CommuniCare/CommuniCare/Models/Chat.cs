using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Chat
{
    public int ChatId { get; set; }

    public int UtilizadorId { get; set; }

    public virtual ICollection<Mensagem> Mensagems { get; set; } = new List<Mensagem>();

    public virtual Utilizador Utilizador { get; set; } = null!;
}
