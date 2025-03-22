using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Utilizador
{
    public int UtilizadorId { get; set; }

    public string? NomeUtilizador { get; set; }

    public string? Password { get; set; }

    public int? NumCares { get; set; }

    public int MoradaId { get; set; }

    public int TipoUtilizadorId { get; set; }

    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();

    public virtual ICollection<Contacto> Contactos { get; set; } = new List<Contacto>();

    public virtual Morada Morada { get; set; } = null!;

    public virtual ICollection<Notificacao> Notificacaos { get; set; } = new List<Notificacao>();

    public virtual ICollection<PedidoAjuda> PedidoAjuda { get; set; } = new List<PedidoAjuda>();

    public virtual TipoUtilizador TipoUtilizador { get; set; } = null!;

    public virtual ICollection<Venda> Venda { get; set; } = new List<Venda>();

    public virtual ICollection<Voluntariado> Voluntariados { get; set; } = new List<Voluntariado>();

    public virtual ICollection<ItemEmprestimo> Items { get; set; } = new List<ItemEmprestimo>();
}
