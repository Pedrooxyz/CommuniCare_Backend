using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    public enum EstadoPedido
    {
        Pendente = 0,
        Aberto = 1,
        EmProgresso = 2,
        Concluido = 3,
        Rejeitado = 4
    }

    public partial class PedidoAjuda
    {

        #region Atributos

        int pedidoId;

        string descPedido;

        int? recompensaCares;

        EstadoPedido estado = EstadoPedido.Aberto;

        DateTime? horarioAjuda;

        int? nHoras;

        int? transacaoId;

        int? nPessoas;

        int? utilizadorId;

        string? fotografiaPA;

        ICollection<Notificacao> notificacaos = new List<Notificacao>();
        
        TransacaoAjuda transacao = null!;
        
        Utilizador utilizador = null!;
        
        ICollection<Voluntariado> voluntariados = new List<Voluntariado>();

        #endregion

        #region Propriedades

        public int PedidoId
        {
            get { return pedidoId; }
            set { pedidoId = value; }
        }

        public string DescPedido
        {
            get { return descPedido; }
            set { descPedido = value; }
        }

        public int? RecompensaCares
        {
            get { return recompensaCares; }
            set { recompensaCares = value; }
        }

        public EstadoPedido Estado
        {
            get { return estado; }
            set { estado = value; }
        }

        public DateTime? HorarioAjuda
        {
            get { return horarioAjuda; }
            set { horarioAjuda = value; }
        }

        public int? NHoras
        {
            get { return nHoras; }
            set { nHoras = value; }
        }

        public int? TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        public int? NPessoas
        {
            get { return nPessoas; }
            set { nPessoas = value; }
        }

        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        public string? FotografiaPA
        {
            get { return fotografiaPA; }
            set { fotografiaPA = value; }
        }

        public virtual ICollection<Notificacao> Notificacaos
        {
            get { return notificacaos; }
            set { notificacaos = value; }
        }

        public virtual TransacaoAjuda Transacao
        {
            get { return transacao; }
            set { transacao = value; }
        }

        public virtual Utilizador Utilizador
        {
            get { return utilizador; }
            set { utilizador = value; }
        }

        public virtual ICollection<Voluntariado> Voluntariados
        {
            get { return voluntariados; }
            set { voluntariados = value; }
        }

        #endregion

        #region Construtores

        public PedidoAjuda(string descPedido, DateTime horarioAjuda, int nHoras, int nPessoas, int utilizadorId)
        {
            DescPedido = descPedido;
            HorarioAjuda = horarioAjuda;
            NHoras = nHoras;
            NPessoas = nPessoas;
            UtilizadorId = utilizadorId;
            RecompensaCares = CalculaRecompensa(nHoras);
        }

        public PedidoAjuda() { }

        #endregion

        #region Métodos

        int CalculaRecompensa(int nHoras)
        {
            return nHoras * 50;
        }

        #endregion
    
    }
}