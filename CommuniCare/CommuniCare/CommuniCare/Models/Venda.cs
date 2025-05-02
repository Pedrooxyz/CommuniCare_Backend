using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Venda
{

    #region Atributos

    int? nArtigos;

    int? utilizadorId;

    int transacaoId;

    ICollection<Artigo> artigos = new List<Artigo>();
    
    Transacao transacao = null!;
    
    Utilizador utilizador = null!;

    #endregion

    #region Propriedades

    public int? NArtigos
    {
        get { return nArtigos; }
        set { nArtigos = value; }
    }

    public int? UtilizadorId
    {
        get { return utilizadorId; }
        set { utilizadorId = value; }
    }

    public int TransacaoId
    {
        get { return transacaoId; }
        set { transacaoId = value; }
    }

    public virtual ICollection<Artigo> Artigos
    {
        get { return artigos; }
        set { artigos = value; }
    }

    public virtual Transacao Transacao
    {
        get { return transacao; }
        set { transacao = value; }
    }

    public virtual Utilizador Utilizador
    {
        get { return utilizador; }
        set { utilizador = value; }
    }

    #endregion

}
