/// <summary>
/// Namespace que contém todas as classes do modelo de dados da aplicação CommuniCare.
/// </summary>
/// <remarks>
/// Este namespace é utilizado para agrupar todas as classes de modelo que representam as entidades e as suas relações dentro da aplicação CommuniCare.
/// </remarks>
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

namespace CommuniCare.Models{
    
    /// <summary>
    /// Representa os estados possíveis de um utilizador no sistema.
    /// </summary>
    /// <remarks>
    /// Este enum define os diferentes estados que um utilizador pode ter dentro do sistema. 
    /// Pode estar pendente (inativo), ativo ou inativo.
    /// </remarks>
    public enum EstadoUtilizador
    {
        /// <summary>
        /// Utilizador com estado pendente, aguardando ativação ou ação.
        /// </summary>
        Pendente = 0,

        /// <summary>
        /// Utilizador ativo, com acesso total ao sistema.
        /// </summary>
        Ativo = 1,

        /// <summary>
        /// Utilizador inativo, sem permissão de acesso ao sistema.
        /// </summary>
        Inativo = 2
    }

    /// <summary>
    /// Representa um utilizador da aplicação CommuniCare.
    /// </summary>
    /// <remarks>
    /// Esta classe modela um utilizador, incluindo as suas propriedades pessoais, estado de conta, 
    /// tipo de utilizador e suas relações com outras entidades, como pedidos de ajuda, notificações, 
    /// contactos, chats, entre outras.
    /// </remarks>
    public partial class Utilizador
    {

        #region Atributos

        /// <summary>
        /// ID único do utilizador.
        /// </summary>
        int utilizadorId;

        /// <summary>
        /// Nome do utilizador.
        /// </summary>
        string? nomeUtilizador;

        /// <summary>
        /// Senha do utilizador.
        /// </summary>
        string? password;

        /// <summary>
        /// Foto do utilizador.
        /// </summary>
        string? fotoUtil;

        /// <summary>
        /// Número de Cares (pontuação de participação) do utilizador.
        /// </summary>
        int? numCares;

        /// <summary>
        /// ID da morada do utilizador.
        /// </summary>
        int moradaId;

        /// <summary>
        /// ID do tipo de utilizador.
        /// </summary>
        int tipoUtilizadorId;

        /// <summary>
        /// Estado do utilizador no sistema (Pendente, Ativo ou Inativo).
        /// </summary>
        EstadoUtilizador estadoUtilizador = EstadoUtilizador.Pendente;

        /// <summary>
        /// Coleção de chats em que o utilizador está envolvido.
        /// </summary>
        ICollection<Chat> chats = new List<Chat>();
        
        /// <summary>
        /// Coleção de contactos do utilizador.
        /// </summary>
        ICollection<Contacto> contactos = new List<Contacto>();
        
        /// <summary>
        /// Representa a morada do utilizador.
        /// </summary>
        Morada morada = null!;
        
        /// <summary>
        /// Coleção de notificações que o utilizador recebe.
        /// </summary>
        ICollection<Notificacao> notificacaos = new List<Notificacao>();
        
        /// <summary>
        /// Coleção de pedidos de ajuda feitos pelo utilizador.
        /// </summary>
        ICollection<PedidoAjuda> pedidoAjuda = new List<PedidoAjuda>();
        
        /// <summary>
        /// Representa o tipo de utilizador.
        /// </summary>
        TipoUtilizador tipoUtilizador = null!;
        
        /// <summary>
        /// Coleção de vendas feitas pelo utilizador.
        /// </summary>
        ICollection<Venda> venda = new List<Venda>();
        
        /// <summary>
        /// Coleção de voluntariados feitos pelo utilizador.
        /// </summary>
        ICollection<Voluntariado> voluntariados = new List<Voluntariado>();
        
        /// <summary>
        /// Coleção de itens de empréstimo associados ao utilizador.
        /// </summary>
        ICollection<ItemEmprestimo> itensEmprestimo = new List<ItemEmprestimo>();
        
        /// <summary>
        /// Coleção de itens de empréstimo associados ao utilizador em relação com outros utilizadores.
        /// </summary>
        ICollection<ItemEmprestimoUtilizador> itemEmprestimoUtilizadores = new List<ItemEmprestimoUtilizador>();
        
        /// <summary>
        /// Coleção de artigos favoritos do utilizador.
        /// </summary>
        ICollection<Favoritos> artigosFavoritos = new List<Favoritos>();

        #endregion

        #region Propriedades

        /// <summary>
        /// Obtém ou define o ID único do utilizador.
        /// </summary>
        public int UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        /// <summary>
        /// Obtém ou define o nome do utilizador.
        /// </summary>
        public string? NomeUtilizador
        {
            get { return nomeUtilizador; }
            set { nomeUtilizador = value; }
        }

        /// <summary>
        /// Obtém ou define a senha do utilizador.
        /// </summary>
        public string? Password
        {
            get { return password; }
            set { password = value; }
        }

        /// <summary>
        /// Obtém ou define a foto do utilizador.
        /// </summary>
        public string? FotoUtil
        {
            get { return fotoUtil; }
            set { fotoUtil = value; }
        }

        /// <summary>
        /// Obtém ou define o número de Cares (pontuação de participação) do utilizador.
        /// </summary>
        public int? NumCares
        {
            get { return numCares; }
            set { numCares = value; }
        }

        /// <summary>
        /// Obtém ou define o ID da morada do utilizador.
        /// </summary>
        public int MoradaId
        {
            get { return moradaId; }
            set { moradaId = value; }
        }

        /// <summary>
        /// Obtém ou define o ID do tipo de utilizador.
        /// </summary>
        public int TipoUtilizadorId
        {
            get { return tipoUtilizadorId; }
            set { tipoUtilizadorId = value; }
        }

        /// <summary>
        /// Obtém ou define o estado do utilizador no sistema.
        /// </summary>
        public EstadoUtilizador EstadoUtilizador
        {
            get { return estadoUtilizador; }
            set { estadoUtilizador = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de chats em que o utilizador está envolvido.
        /// </summary>
        public virtual ICollection<Chat> Chats
        {
            get { return chats; }
            set { chats = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de contactos do utilizador.
        /// </summary>
        public virtual ICollection<Contacto> Contactos
        {
            get { return contactos; }
            set { contactos = value; }
        }

        /// <summary>
        /// Obtém ou define a morada do utilizador.
        /// </summary>
        public virtual Morada Morada
        {
            get { return morada; }
            set { morada = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de notificações do utilizador.
        /// </summary>
        public virtual ICollection<Notificacao> Notificacaos
        {
            get { return notificacaos; }
            set { notificacaos = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de pedidos de ajuda feitos pelo utilizador.
        /// </summary>
        public virtual ICollection<PedidoAjuda> PedidoAjuda
        {
            get { return pedidoAjuda; }
            set { pedidoAjuda = value; }
        }

        /// <summary>
        /// Obtém ou define o tipo de utilizador.
        /// </summary>
        public virtual TipoUtilizador TipoUtilizador
        {
            get { return tipoUtilizador; }
            set { tipoUtilizador = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de vendas feitas pelo utilizador.
        /// </summary>
        public virtual ICollection<Venda> Venda
        {
            get { return venda; }
            set { venda = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de voluntariados feitos pelo utilizador.
        /// </summary>
        public virtual ICollection<Voluntariado> Voluntariados
        {
            get { return voluntariados; }
            set { voluntariados = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de itens de empréstimo associados ao utilizador.
        /// </summary>
        public virtual ICollection<ItemEmprestimo> ItensEmprestimo
        {
            get { return itensEmprestimo; }
            set { itensEmprestimo = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de itens de empréstimo associados ao utilizador em relação com outros utilizadores.
        /// </summary>
        public virtual ICollection<ItemEmprestimoUtilizador> ItemEmprestimoUtilizadores
        {
            get { return itemEmprestimoUtilizadores; }
            set { itemEmprestimoUtilizadores = value; }
        }

        /// <summary>
        /// Obtém ou define a coleção de artigos favoritos do utilizador.
        /// </summary>
        public virtual ICollection<Favoritos> ArtigosFavoritos
        {
            get { return artigosFavoritos; }
            set { artigosFavoritos = value; }
        }

        #endregion

        #region Metodos

        /// <summary>
        /// Construtor da classe Utilizador.
        /// </summary>
        /// <remarks>
        /// Este construtor é utilizado para inicializar uma nova instância da classe Utilizador.
        /// Ele não recebe parâmetros, mas prepara a instância para ser configurada com valores específicos posteriormente.
        /// </remarks>
        public Utilizador() { }

        /// <summary>
        /// Método que permite a um utilizador criar um pedido de ajuda.
        /// </summary>
        /// <param name="descPedido">Descrição do pedido de ajuda.</param>
        /// <param name="horarioAjuda">Data e hora programada para a ajuda.</param>
        /// <param name="nHoras">Número de horas estimado para o auxílio.</param>
        /// <param name="nPessoas">Número de pessoas necessárias para a ajuda.</param>
        /// <param name="utilizadorId">ID do utilizador que está a pedir ajuda.</param>
        /// <returns>Retorna um valor booleano indicando se o pedido foi criado com sucesso.</returns>
        /// <remarks>
        /// Este método cria uma nova instância da classe PedidoAjuda e a adiciona à coleção de pedidos do utilizador.
        /// Caso ocorra algum erro durante a criação do pedido, o método retorna false.
        /// </remarks>
        public bool PedirAjuda(string descPedido, DateTime horarioAjuda, int nHoras, int nPessoas, int utilizadorId)
        {
            try
            {
                PedidoAjuda pedido = new PedidoAjuda(descPedido, horarioAjuda, nHoras, nPessoas, utilizadorId);
                this.PedidoAjuda.Add(pedido);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }

}