using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class PedidoAjudum
{
    public int PedidoId { get; set; }

    public string? DescPedido { get; set; }

    public int? RecompensaCares { get; set; }

    public byte? Estado { get; set; }

    public DateTime? HorarioAjuda { get; set; }

    public int? NHoras { get; set; }

    public int TransacaoId { get; set; }

    public int? NPessoas { get; set; }

    public int UtilizadorId { get; set; }

    public virtual ICollection<Notificacao> Notificacaos { get; set; } = new List<Notificacao>();

    public virtual TransacaoAjudum Transacao { get; set; } = null!;

    public virtual Utilizador Utilizador { get; set; } = null!;

    public virtual ICollection<Voluntariado> Voluntariados { get; set; } = new List<Voluntariado>();
}
