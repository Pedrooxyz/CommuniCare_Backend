using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Emprestimo
{
    public int EmprestimoId { get; set; }

    public DateTime? DataIni { get; set; }

    public DateTime? DataDev { get; set; }

    public int? TransacaoId { get; set; }

    public virtual TransacaoEmprestimo Transacao { get; set; } = null!;

    public virtual ICollection<ItemEmprestimo> Items { get; set; } = new List<ItemEmprestimo>();
}
