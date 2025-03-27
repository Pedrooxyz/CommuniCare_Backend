using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public enum EstadoPedido
{
    Aberto = 0,
    EmProgresso = 1,
    Concluido = 2
}

public partial class PedidoAjuda
{
    public int PedidoId { get; set; }

    public string DescPedido { get; set; }

    public int? RecompensaCares { get; set; }

    public EstadoPedido Estado { get; set; } = EstadoPedido.Aberto;

    public DateTime? HorarioAjuda { get; set; }

    public int? NHoras { get; set; }

    public int TransacaoId { get; set; }

    public int? NPessoas { get; set; }

    public int UtilizadorId { get; set; }

    public virtual ICollection<Notificacao> Notificacaos { get; set; } = new List<Notificacao>();

    public virtual TransacaoAjuda Transacao { get; set; } = null!;

    public virtual Utilizador Utilizador { get; set; } = null!;

    public virtual ICollection<Voluntariado> Voluntariados { get; set; } = new List<Voluntariado>();

    public PedidoAjuda (string descPedido, DateTime horarioAjuda, int nHoras, int nPessoas, int utilizadorId)
    {
        DescPedido=descPedido;
        HorarioAjuda=horarioAjuda;
        NHoras=nHoras;
        NPessoas=nPessoas;
        UtilizadorId=utilizadorId;
        RecompensaCares = CalculaRecompensa(nHoras);

    }

    public PedidoAjuda() { }


    int CalculaRecompensa(int nHoras)
    {
        return nHoras * 50;
    }

    #region Metodos



    #endregion
}
