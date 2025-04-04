using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Mensagem
{
    public int MensagemId { get; set; }

    public string? Conteudo { get; set; }

    public DateTime? DataEnvio { get; set; }

    public int ChatId { get; set; }

    public virtual Chat Chat { get; set; } = null!;
}
