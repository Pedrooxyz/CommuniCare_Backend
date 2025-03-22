using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class TransacaoEmprestimo
{
    public int? NHoras { get; set; }

    public int? RecetorTran { get; set; }

    public int? PagaTran { get; set; }

    public int TransacaoId { get; set; }

    public virtual ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();

    public virtual Transacao Transacao { get; set; } = null!;
}
