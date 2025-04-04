using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Transacao
{
    public int TransacaoId { get; set; }

    public DateTime? DataTransacao { get; set; }

    public int? Quantidade { get; set; }

    public virtual TransacaoAjuda? TransacaoAjuda { get; set; }

    public virtual TransacaoEmprestimo? TransacaoEmprestimo { get; set; }

    public virtual Venda? Venda { get; set; }
}
