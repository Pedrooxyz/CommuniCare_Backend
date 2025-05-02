using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

namespace CommuniCare.Models{

    public enum EstadoUtilizador
    {
        Pendente = 0,
        Ativo = 1,
        Inativo = 2
    }

    public partial class Utilizador
    {

        #region Atributos

        int utilizadorId;

        string? nomeUtilizador;

        string? password;

        string? fotoUtil;

        int? numCares;

        int moradaId;

        int tipoUtilizadorId;

        EstadoUtilizador estadoUtilizador = EstadoUtilizador.Pendente;
        
        ICollection<Chat> chats = new List<Chat>();
        
        ICollection<Contacto> contactos = new List<Contacto>();
        
        Morada morada = null!;
        
        ICollection<Notificacao> notificacaos = new List<Notificacao>();
        
        ICollection<PedidoAjuda> pedidoAjuda = new List<PedidoAjuda>();
        
        TipoUtilizador tipoUtilizador = null!;
        
        ICollection<Venda> venda = new List<Venda>();
        
        ICollection<Voluntariado> voluntariados = new List<Voluntariado>();
        
        ICollection<ItemEmprestimo> itensEmprestimo = new List<ItemEmprestimo>();
        
        ICollection<ItemEmprestimoUtilizador> itemEmprestimoUtilizadores = new List<ItemEmprestimoUtilizador>();
        
        ICollection<Favoritos> artigosFavoritos = new List<Favoritos>();

        #endregion

        #region Propriedades

        public int UtilizadorId
        {
            get { return utilizadorId; }
            set { utilizadorId = value; }
        }

        public string? NomeUtilizador
        {
            get { return nomeUtilizador; }
            set { nomeUtilizador = value; }
        }

        public string? Password
        {
            get { return password; }
            set { password = value; }
        }

        public string? FotoUtil
        {
            get { return fotoUtil; }
            set { fotoUtil = value; }
        }

        public int? NumCares
        {
            get { return numCares; }
            set { numCares = value; }
        }

        public int MoradaId
        {
            get { return moradaId; }
            set { moradaId = value; }
        }

        public int TipoUtilizadorId
        {
            get { return tipoUtilizadorId; }
            set { tipoUtilizadorId = value; }
        }

        public EstadoUtilizador EstadoUtilizador
        {
            get { return estadoUtilizador; }
            set { estadoUtilizador = value; }
        }

        public virtual ICollection<Chat> Chats
        {
            get { return chats; }
            set { chats = value; }
        }

        public virtual ICollection<Contacto> Contactos
        {
            get { return contactos; }
            set { contactos = value; }
        }

        public virtual Morada Morada
        {
            get { return morada; }
            set { morada = value; }
        }

        public virtual ICollection<Notificacao> Notificacaos
        {
            get { return notificacaos; }
            set { notificacaos = value; }
        }

        public virtual ICollection<PedidoAjuda> PedidoAjuda
        {
            get { return pedidoAjuda; }
            set { pedidoAjuda = value; }
        }

        public virtual TipoUtilizador TipoUtilizador
        {
            get { return tipoUtilizador; }
            set { tipoUtilizador = value; }
        }

        public virtual ICollection<Venda> Venda
        {
            get { return venda; }
            set { venda = value; }
        }

        public virtual ICollection<Voluntariado> Voluntariados
        {
            get { return voluntariados; }
            set { voluntariados = value; }
        }

        public virtual ICollection<ItemEmprestimo> ItensEmprestimo
        {
            get { return itensEmprestimo; }
            set { itensEmprestimo = value; }
        }

        public virtual ICollection<ItemEmprestimoUtilizador> ItemEmprestimoUtilizadores
        {
            get { return itemEmprestimoUtilizadores; }
            set { itemEmprestimoUtilizadores = value; }
        }

        public virtual ICollection<Favoritos> ArtigosFavoritos
        {
            get { return artigosFavoritos; }
            set { artigosFavoritos = value; }
        }

        #endregion

        #region Metodos

        public Utilizador() { }

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