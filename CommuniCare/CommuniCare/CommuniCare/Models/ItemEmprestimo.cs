using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class ItemEmprestimo
{
    public int ItemId { get; set; }

    public string? NomeItem { get; set; }

    public string? DescItem { get; set; }

    public byte? Disponivel { get; set; }

    public int? ComissaoCares { get; set; }

    public int idEmprestador { get; set; }

    public virtual ICollection<Notificacao> Notificacaos { get; set; } = new List<Notificacao>();

    public virtual ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();

    public virtual ICollection<Utilizador> Utilizadores { get; set; } = new List<Utilizador>();

    public virtual ICollection<ItemEmprestimoUtilizador> ItemEmprestimoUtilizadores { get; set; }
}
