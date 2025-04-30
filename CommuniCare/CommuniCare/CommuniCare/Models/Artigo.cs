using System;
using System.Collections.Generic;

namespace CommuniCare.Models;


public enum EstadoArtigo
{
    Disponivel,
    Indisponivel
}

public partial class Artigo
{
    
    public int ArtigoId { get; set; }

    public string? NomeArtigo { get; set; }

    public string? DescArtigo { get; set; }

    public int? CustoCares { get; set; }

    public int LojaId { get; set; }

    public int? TransacaoId { get; set; }

    public int QuantidadeDisponivel { get; set; }

    public string? FotografiaArt { get; set; }

    public virtual Loja Loja { get; set; } = null!;

    public virtual Venda? Transacao { get; set; }

    public EstadoArtigo Estado { get; set; } = EstadoArtigo.Disponivel;

    public virtual ICollection<Favoritos> FavoritoPor { get; set; } = new List<Favoritos>();
}
