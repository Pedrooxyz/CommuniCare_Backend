using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public enum EstadoLoja
{
    Inativo = 0,
    Ativo = 1
}



public partial class Loja
{
    public int LojaId { get; set; }

    public string? NomeLoja { get; set; }

    public string? DescLoja { get; set; }

    public EstadoLoja Estado { get; set; } = EstadoLoja.Ativo;

    public virtual ICollection<Artigo> Artigos { get; set; } = new List<Artigo>();
}
