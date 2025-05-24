/// <summary>
/// Este namespace contém os modelos do sistema CommuniCare, incluindo entidades relacionadas com pedidos de ajuda.
/// </summary>
/// 
/// <remarks>
/// A classe <see cref="PedidoAjuda"/> representa um pedido de ajuda realizado por um utilizador, incluindo a descrição do pedido,
/// recompensa oferecida, estado atual, horário da ajuda, entre outras informações.
/// </remarks>
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;

namespace CommuniCare.Models
{
    /// <summary>
    /// Enumeração que representa os possíveis estados de um pedido de ajuda.
    /// </summary>
    public enum EstadoPedido
    {
        /// <summary>
        /// Pedido pendente.
        /// </summary>
        Pendente = 0,

        /// <summary>
        /// Pedido aberto.
        /// </summary>
        Aberto = 1,

        /// <summary>
        /// Pedido em progresso.
        /// </summary>
        EmProgresso = 2,

        /// <summary>
        /// Pedido concluído.
        /// </summary>
        Concluido = 3,

        /// <summary>
        /// Pedido rejeitado.
        /// </summary>
        Rejeitado = 4
    }

    /// <summary>
    /// Representa um pedido de ajuda realizado por um utilizador no sistema.
    /// </summary>
    public partial class PedidoAjuda
    {

        #region Atributos

        /// <summary>
        /// Identificador único do pedido de ajuda.
        /// </summary>
        int pedidoId;

        /// <summary>
        /// Titulo do Pedido de Ajuda.
        /// </summary>
        string? titulo;

        /// <summary>
        /// Descrição do pedido de ajuda.
        /// </summary>
        string descPedido;

        /// <summary>
        /// Recompensa oferecida pelo utilizador em Cares.
        /// </summary>
        int? recompensaCares;

        /// <summary>
        /// Estado atual do pedido de ajuda.
        /// </summary>
        EstadoPedido estado = EstadoPedido.Aberto;

        /// <summary>
        /// Data e hora programada para o início da ajuda.
        /// </summary>
        DateTime? horarioAjuda;

        /// <summary>
        /// Número de horas estimadas para a ajuda.
        /// </summary>
        int? nHoras;

        /// <summary>
        /// Identificador da transação associada ao pedido, se houver.
        /// </summary>
        int? transacaoId;

        /// <summary>
        /// Número de pessoas necessárias para o pedido.
        /// </summary>
        int? nPessoas;

        /// <summary>
        /// Identificador do utilizador que fez o pedido.
        /// </summary>
        int? utilizadorId;

        /// <summary>
        /// Fotografia associada ao pedido de ajuda.
        /// </summary>
        string? fotografiaPA;

        /// <summary>
        /// Coleção de notificações associadas ao pedido de ajuda.
        /// </summary>
        ICollection<Notificacao> notificacaos = new List<Notificacao>();
        
        /// <summary>
        /// Transação associada ao pedido de ajuda.
        /// </summary>
        TransacaoAjuda transacao = null!;
        
        /// <summary>
        /// Utilizador que fez o pedido de ajuda.
        /// </summary>
        Utilizador utilizador = null!;
        
        /// <summary>
        /// Coleção de voluntariados associados ao pedido de ajuda.
        /// </summary>
        ICollection<Voluntariado> voluntariados = new List<Voluntariado>();

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o identificador único do pedido de ajuda.
        /// </summary>
        public int PedidoId
        {
            get { return pedidoId; }
            set { pedidoId = value; }
        }

        /// <summary>
        /// Obtém ou define a descrição do pedido de ajuda.
        /// </summary>
        public string DescPedido
        {
            get { return descPedido; }
            set { descPedido = value; }
        }

        /// <summary>
        /// Obtém ou define a recompensa oferecida pelo utilizador em Cares.
        /// </summary>
        public int? RecompensaCares
        {
            get { return recompensaCares; }
            set { recompensaCares = value; }
        }

        /// <summary>
        /// Obtém ou define a recompensa oferecida pelo utilizador em Cares.
        /// </summary>
        public EstadoPedido Estado
        {
            get { return estado; }
            set { estado = value; }
        }

        /// <summary>
        /// Obtém ou define o horário programado para o início da ajuda.
        /// </summary>
        public DateTime? HorarioAjuda
        {
            get { return horarioAjuda; }
            set { horarioAjuda = value; }
        }

        /// <summary>
        /// Obtém ou define o número de horas estimadas para o pedido de ajuda.
        /// </summary>
        public int? NHoras
        {
            get { return nHoras; }
            set { nHoras = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador da transação associada ao pedido.
        /// </summary>
        public int? TransacaoId
        {
            get { return transacaoId; }
            set { transacaoId = value; }
        }

        /// <summary>
        /// Obtém ou define o número de pessoas necessárias para o pedido de ajuda.
        /// </summary>
        public int? NPessoas
        {
            get { return nPessoas; }
            set { nPessoas = value; }
        }

        /// <summary>
        /// Obtém ou define o identificador do utilizador que fez o pedido.
        /// </summary>
        public int? UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        public string? Titulo
        {
            get { return titulo; }
            set { titulo = value; }
        }

        /// <summary>
        /// Obtém ou define a fotografia associada ao pedido de ajuda.
        /// </summary>
        public string? FotografiaPA
        {
            get { return fotografiaPA; }
            set { fotografiaPA = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de notificações associadas ao pedido de ajuda.
        /// </summary>
        public virtual ICollection<Notificacao> Notificacaos
        {
            get { return notificacaos; }
            set { notificacaos = value; }
        }

        /// <summary>
        /// Obtém ou define a transação associada ao pedido de ajuda.
        /// </summary>
        public virtual TransacaoAjuda Transacao
        {
            get { return transacao; }
            set { transacao = value; }
        }

        /// <summary>
        /// Obtém ou define o utilizador que fez o pedido de ajuda.
        /// </summary>
        public virtual Utilizador Utilizador
        {
            get { return utilizador; }
            set { utilizador = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de voluntariados associados ao pedido de ajuda.
        /// </summary>
        public virtual ICollection<Voluntariado> Voluntariados
        {
            get { return voluntariados; }
            set { voluntariados = value; }
        }

        #endregion

        #region Construtores

        /// <summary>
        /// Construtor da classe <see cref="PedidoAjuda"/>.
        /// </summary>
        /// <param name="descPedido">Descrição do pedido de ajuda.</param>
        /// <param name="horarioAjuda">Horário da ajuda programada.</param>
        /// <param name="nHoras">Número de horas estimadas para o pedido.</param>
        /// <param name="nPessoas">Número de pessoas necessárias para o pedido.</param>
        /// <param name="utilizadorId">Identificador do utilizador que fez o pedido.</param>
        public PedidoAjuda(string descPedido, DateTime horarioAjuda, int nHoras, int nPessoas, int utilizadorId)
        {
            DescPedido = descPedido;
            HorarioAjuda = horarioAjuda;
            NHoras = nHoras;
            NPessoas = nPessoas;
            UtilizadorId = utilizadorId;
            RecompensaCares = CalculaRecompensa(nHoras);
        }

        /// <summary>
        /// Construtor vazio para a classe <see cref="PedidoAjuda"/>.
        /// </summary>
        public PedidoAjuda() { }

        #endregion

        #region Métodos

        /// <summary>
        /// Calcula a recompensa em Cares com base no número de horas.
        /// </summary>
        /// <param name="nHoras">Número de horas do pedido de ajuda.</param>
        /// <returns>Valor da recompensa em Cares.</returns>
        int CalculaRecompensa(int nHoras)
        {
            return nHoras * 50;
        }


        #endregion
    
    }
}