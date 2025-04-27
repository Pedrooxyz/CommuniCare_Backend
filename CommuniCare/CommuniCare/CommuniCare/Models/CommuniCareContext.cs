using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CommuniCare.Models;

public partial class CommuniCareContext : DbContext
{
    public CommuniCareContext()
    {
    }

    public CommuniCareContext(DbContextOptions<CommuniCareContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Artigo> Artigos { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Contacto> Contactos { get; set; }

    public virtual DbSet<Cp> Cps { get; set; }

    public virtual DbSet<Emprestimo> Emprestimos { get; set; }

    public virtual DbSet<Favoritos> Favoritos { get; set; }

    public virtual DbSet<ItemEmprestimo> ItensEmprestimo { get; set; }

    public virtual DbSet<Loja> Lojas { get; set; }

    public virtual DbSet<Mensagem> Mensagens { get; set; }

    public virtual DbSet<Morada> Morada { get; set; }

    public virtual DbSet<Notificacao> Notificacaos { get; set; }

    public virtual DbSet<PedidoAjuda> PedidosAjuda { get; set; }

    public virtual DbSet<TipoContacto> TipoContactos { get; set; }

    public virtual DbSet<TipoUtilizador> TipoUtilizadors { get; set; }

    public virtual DbSet<Transacao> Transacoes { get; set; }

    public virtual DbSet<TransacaoAjuda> TransacaoAjuda { get; set; }

    public virtual DbSet<TransacaoEmprestimo> TransacoesEmprestimo { get; set; }

    public virtual DbSet<Utilizador> Utilizadores { get; set; }

    public virtual DbSet<Venda> Venda { get; set; }

    public virtual DbSet<Voluntariado> Voluntariados { get; set; }

    public virtual DbSet<ItemEmprestimoUtilizador> ItemEmprestimoUtilizadores { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263. 
        => optionsBuilder.UseSqlServer("Data Source=localhost; Database=CommuniCare; Integrated Security=True; Encrypt=False");


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artigo>(entity =>
        {
            entity.HasKey(e => e.ArtigoId).HasName("PK__Artigo__34661042A75121D9");

            entity.ToTable("Artigo");

            entity.Property(e => e.ArtigoId).HasColumnName("artigoID");
            entity.Property(e => e.CustoCares).HasColumnName("custoCares");
            entity.Property(e => e.DescArtigo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descArtigo");
            entity.Property(e => e.LojaId).HasColumnName("lojaID");
            entity.Property(e => e.NomeArtigo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nomeArtigo");
            entity.Property(e => e.TransacaoId).HasColumnName("transacaoID");

            entity.HasOne(d => d.Loja).WithMany(p => p.Artigos)
                .HasForeignKey(d => d.LojaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKArtigo399404");

            entity.HasOne(d => d.Transacao).WithMany(p => p.Artigos)
                .HasForeignKey(d => d.TransacaoId)
                .HasConstraintName("FKArtigo680902");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("PK__Chat__8263854DE07F2A4F");

            entity.ToTable("Chat");

            entity.Property(e => e.ChatId).HasColumnName("chatID");
            entity.Property(e => e.UtilizadorId).HasColumnName("utilizadorID");

            entity.HasOne(d => d.Utilizador).WithMany(p => p.Chats)
                .HasForeignKey(d => d.UtilizadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKChat902984");
        });

        modelBuilder.Entity<Contacto>(entity =>
        {
            entity.HasKey(e => e.ContactoId).HasName("PK__Contacto__0ECCADC7FB690AAD");

            entity.ToTable("Contacto");

            entity.Property(e => e.ContactoId).HasColumnName("contactoID");
            entity.Property(e => e.NumContacto)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("numContacto");
            entity.Property(e => e.TipoContactoId).HasColumnName("tipoContactoID");
            entity.Property(e => e.UtilizadorId).HasColumnName("utilizadorID");

            entity.HasOne(d => d.TipoContacto).WithMany(p => p.Contactos)
                .HasForeignKey(d => d.TipoContactoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKContacto906402");

            entity.HasOne(d => d.Utilizador).WithMany(p => p.Contactos)
                .HasForeignKey(d => d.UtilizadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKContacto995974");
        });

        modelBuilder.Entity<Cp>(entity =>
        {
            entity.HasKey(e => e.CPostal).HasName("PK__CP__F5B22BE63663255C");

            entity.ToTable("CP");

            entity.Property(e => e.CPostal).HasColumnName("CPostal");
            entity.Property(e => e.Localidade).HasColumnName("Localidade");
        });

        modelBuilder.Entity<Emprestimo>(entity =>
        {
            entity.HasKey(e => e.EmprestimoId).HasName("PK__Empresti__D470E7830BFF4320");

            entity.ToTable("Emprestimo");

            entity.Property(e => e.EmprestimoId).HasColumnName("emprestimoID");
            entity.Property(e => e.DataDev)
                .HasColumnType("datetime")
                .HasColumnName("dataDev");
            entity.Property(e => e.DataIni)
                .HasColumnType("datetime")
                .HasColumnName("dataIni");
            entity.Property(e => e.TransacaoId).HasColumnName("transacaoID");

            entity.HasOne(d => d.Transacao).WithMany(p => p.Emprestimos)
                .HasForeignKey(d => d.TransacaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKEmprestimo891193");
        });

        modelBuilder.Entity<Favoritos>(f =>
    {
        f.HasKey(x => new { x.UtilizadorId, x.ArtigoId });

        f.HasOne(x => x.Utilizador)
         .WithMany(u => u.ArtigosFavoritos)
         .HasForeignKey(x => x.UtilizadorId)
         .OnDelete(DeleteBehavior.Cascade);

        f.HasOne(x => x.Artigo)
         .WithMany(a => a.FavoritoPor)
         .HasForeignKey(x => x.ArtigoId)
         .OnDelete(DeleteBehavior.Cascade);
    });


        modelBuilder.Entity<ItemEmprestimo>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__ItemEmpr__56A1284AB54FF227");

            entity.ToTable("ItemEmprestimo");

            entity.Property(e => e.ItemId).HasColumnName("itemID");
            entity.Property(e => e.ComissaoCares).HasColumnName("comissaoCares");
            entity.Property(e => e.DescItem)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descItem");
            entity.Property(e => e.Disponivel).HasColumnName("disponivel");
            entity.Property(e => e.NomeItem)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nomeItem");

            entity.HasMany(d => d.Emprestimos).WithMany(p => p.Items)
                .UsingEntity<Dictionary<string, object>>(
                    "EmprestimoItemEmprestimo",
                    r => r.HasOne<Emprestimo>().WithMany()
                        .HasForeignKey("EmprestimoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FKEmprestimo135708"),
                    l => l.HasOne<ItemEmprestimo>().WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FKEmprestimo851433"),
                    j =>
                    {
                        j.HasKey("ItemId", "EmprestimoId").HasName("PK__Empresti__7BE62632FBE4295A");
                        j.ToTable("Emprestimo_ItemEmprestimo");
                        j.IndexerProperty<int>("ItemId").HasColumnName("itemID");
                        j.IndexerProperty<int>("EmprestimoId").HasColumnName("emprestimoID");
                    });

            entity.HasMany(d => d.Utilizadores)
                      .WithMany(p => p.ItensEmprestimo)
                      .UsingEntity<ItemEmprestimoUtilizador>(
              j => j.HasOne(ue => ue.Utilizador)
                    .WithMany(u => u.ItemEmprestimoUtilizadores)
                    .HasForeignKey(ue => ue.UtilizadorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ItemEmprestimo_Utilizador_Utilizador"),

              j => j.HasOne(ue => ue.ItemEmprestimo)
                    .WithMany(i => i.ItemEmprestimoUtilizadores)
                    .HasForeignKey(ue => ue.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ItemEmprestimo_Utilizador_ItemEmprestimo"),

              j =>
              {
                  j.HasKey(ue => new { ue.ItemId, ue.UtilizadorId })
                    .HasName("PK_ItemEmprestimo_Utilizador");

                  j.ToTable("ItemEmprestimoUtilizador");

                  j.Property(ue => ue.ItemId).HasColumnName("itemID");
                  j.Property(ue => ue.UtilizadorId).HasColumnName("utilizadorID");

                  j.Property(ue => ue.TipoRelacao)
                    .HasColumnName("tipoRelacao")
                    .HasMaxLength(10)
                    .IsRequired();
              });

        });

        modelBuilder.Entity<Loja>(entity =>
        {
            entity.HasKey(e => e.LojaId).HasName("PK__Loja__E00D7703297E42E3");

            entity.ToTable("Loja");

            entity.Property(e => e.LojaId).HasColumnName("lojaID");
            entity.Property(e => e.DescLoja)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descLoja");
            entity.Property(e => e.NomeLoja)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nomeLoja");
        });

        modelBuilder.Entity<Mensagem>(entity =>
        {
            entity.HasKey(e => e.MensagemId).HasName("PK__Mensagem__8419FF1A3FB0425E");

            entity.ToTable("Mensagem");

            entity.Property(e => e.MensagemId).HasColumnName("mensagemID");
            entity.Property(e => e.ChatId).HasColumnName("chatID");
            entity.Property(e => e.Conteudo).HasColumnName("conteudo");
            entity.Property(e => e.DataEnvio)
                .HasColumnType("datetime")
                .HasColumnName("dataEnvio");

            entity.HasOne(d => d.Chat).WithMany(p => p.Mensagems)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKMensagem208498");
        });

        modelBuilder.Entity<Morada>(entity =>
        {
            entity.HasKey(e => e.MoradaId).HasName("PK__Morada__5BDD9AB29A5F23DC");

            entity.Property(e => e.MoradaId).HasColumnName("moradaID");
            entity.Property(e => e.CPostal).HasColumnName("CPID");
            entity.Property(e => e.NumPorta).HasColumnName("numPorta");
            entity.Property(e => e.Rua)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rua");

            entity.HasOne(d => d.Cp).WithMany(p => p.Morada)
                .HasForeignKey(d => d.CPostal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKMorada368657");
        });

        modelBuilder.Entity<Notificacao>(entity =>
        {
            entity.HasKey(e => e.NotificacaoId).HasName("PK__Notifica__A917F5716C707642");

            entity.ToTable("Notificacao");

            entity.Property(e => e.NotificacaoId).HasColumnName("notificacaoID");
            entity.Property(e => e.DataMensagem)
                .HasColumnType("datetime")
                .HasColumnName("dataMensagem");
            entity.Property(e => e.ItemId).HasColumnName("itemID");
            entity.Property(e => e.Lida).HasColumnName("lida");
            entity.Property(e => e.Mensagem)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("mensagem");
            entity.Property(e => e.PedidoId).HasColumnName("pedidoID");
            entity.Property(e => e.UtilizadorId).HasColumnName("utilizadorID");

            entity.HasOne(d => d.Item).WithMany(p => p.Notificacaos)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKNotificaca657866");

            entity.HasOne(d => d.Pedido).WithMany(p => p.Notificacaos)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKNotificaca469461");

            entity.HasOne(d => d.Utilizador).WithMany(p => p.Notificacaos)
                .HasForeignKey(d => d.UtilizadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKNotificaca99191");
        });

        modelBuilder.Entity<PedidoAjuda>(entity =>
        {
            entity.HasKey(e => e.PedidoId).HasName("PK__PedidoAj__BAF07AE4789D03F6");

            entity.Property(e => e.PedidoId).HasColumnName("pedidoID");
            entity.Property(e => e.DescPedido)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descPedido");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.HorarioAjuda)
                .HasColumnType("datetime")
                .HasColumnName("horarioAjuda");
            entity.Property(e => e.NHoras).HasColumnName("nHoras");
            entity.Property(e => e.NPessoas).HasColumnName("nPessoas");
            entity.Property(e => e.RecompensaCares).HasColumnName("recompensaCares");
            entity.Property(e => e.TransacaoId).HasColumnName("transacaoID");
            entity.Property(e => e.UtilizadorId).HasColumnName("utilizadorID");

            entity.HasOne(d => d.Transacao).WithMany(p => p.PedidoAjuda)
                .HasForeignKey(d => d.TransacaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKPedidoAjud864110");

            entity.HasOne(d => d.Utilizador).WithMany(p => p.PedidoAjuda)
                .HasForeignKey(d => d.UtilizadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKPedidoAjud452367");
        });

        modelBuilder.Entity<TipoContacto>(entity =>
        {
            entity.HasKey(e => e.TipoContactoId).HasName("PK__TipoCont__CC154D73604D5E90");

            entity.ToTable("TipoContacto");

            entity.Property(e => e.TipoContactoId).HasColumnName("tipoContactoID");
            entity.Property(e => e.DescContacto)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descContacto");
        });

        modelBuilder.Entity<TipoUtilizador>(entity =>
        {
            entity.HasKey(e => e.TipoUtilizadorId).HasName("PK__TipoUtil__A479CAE646541112");

            entity.ToTable("TipoUtilizador");

            entity.Property(e => e.TipoUtilizadorId).HasColumnName("tipoUtilizadorID");
            entity.Property(e => e.DescTU).HasColumnName("descTU");
        });

        modelBuilder.Entity<Transacao>(entity =>
        {
            entity.HasKey(e => e.TransacaoId).HasName("PK__Transaca__EA9168CEEAA2B152");

            entity.ToTable("Transacao");

            entity.Property(e => e.TransacaoId).HasColumnName("transacaoID");
            entity.Property(e => e.DataTransacao).HasColumnName("dataTransacao");
            entity.Property(e => e.Quantidade).HasColumnName("quantidade");
        });

        modelBuilder.Entity<TransacaoAjuda>(entity =>
        {
            entity.HasKey(e => e.TransacaoId).HasName("PK__Transaca__EA9168CE1D5A6645");

            entity.Property(e => e.TransacaoId)
                .ValueGeneratedNever()
                .HasColumnName("transacaoID");
            entity.Property(e => e.RecetorTran).HasColumnName("recetorTran");

            entity.HasOne(d => d.Transacao).WithOne(p => p.TransacaoAjuda)
                .HasForeignKey<TransacaoAjuda>(d => d.TransacaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKTransacaoA7406");
        });

        modelBuilder.Entity<TransacaoEmprestimo>(entity =>
        {
            entity.HasKey(e => e.TransacaoId).HasName("PK__Transaca__EA9168CEB369504C");

            entity.Property(e => e.TransacaoId)
                .ValueGeneratedNever()
                .HasColumnName("transacaoID");
            entity.Property(e => e.NHoras).HasColumnName("nHoras");
            entity.Property(e => e.PagaTran).HasColumnName("pagaTran");
            entity.Property(e => e.RecetorTran).HasColumnName("recetorTran");

            entity.HasOne(d => d.Transacao).WithOne(p => p.TransacaoEmprestimo)
                .HasForeignKey<TransacaoEmprestimo>(d => d.TransacaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKTransacaoE589588");
        });

        modelBuilder.Entity<Utilizador>(entity =>
        {
            entity.HasKey(e => e.UtilizadorId).HasName("PK__Utilizad__F61D8917D5DAE25F");

            entity.ToTable("Utilizador");

            entity.Property(e => e.UtilizadorId).HasColumnName("utilizadorID");
            entity.Property(e => e.MoradaId).HasColumnName("moradaID");
            entity.Property(e => e.NomeUtilizador)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nomeUtilizador");
            entity.Property(e => e.NumCares).HasColumnName("numCares");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.TipoUtilizadorId).HasColumnName("tipoUtilizadorID");

            entity.HasOne(d => d.Morada).WithMany(p => p.Utilizadores)
                .HasForeignKey(d => d.MoradaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKUtilizador388471");

            entity.HasOne(d => d.TipoUtilizador).WithMany(p => p.Utilizadors)
                .HasForeignKey(d => d.TipoUtilizadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKUtilizador319669");
        });

        modelBuilder.Entity<Venda>(entity =>
        {
            entity.HasKey(e => e.TransacaoId).HasName("PK__Venda__EA9168CECCA8D27D");

            entity.Property(e => e.TransacaoId)
                .ValueGeneratedNever()
                .HasColumnName("transacaoID");
            entity.Property(e => e.NArtigos).HasColumnName("nArtigos");
            entity.Property(e => e.UtilizadorId).HasColumnName("utilizadorID");

            entity.HasOne(d => d.Transacao).WithOne(p => p.Venda)
                .HasForeignKey<Venda>(d => d.TransacaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKVenda552087");

            entity.HasOne(d => d.Utilizador).WithMany(p => p.Venda)
                .HasForeignKey(d => d.UtilizadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKVenda344605");
        });

        modelBuilder.Entity<Voluntariado>(entity =>
        {
            entity.HasKey(e => new { e.PedidoId, e.UtilizadorId, e.IdVoluntariado }).HasName("PK__Voluntar__A4B38AAC16F79537");

            entity.ToTable("Voluntariado");

            entity.Property(e => e.PedidoId).HasColumnName("pedidoID");
            entity.Property(e => e.UtilizadorId).HasColumnName("utilizadorID");
            entity.Property(e => e.IdVoluntariado).HasColumnName("idVoluntariado");

            entity.HasOne(d => d.Pedido).WithMany(p => p.Voluntariados)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKVoluntaria267537");

            entity.HasOne(d => d.Utilizador).WithMany(p => p.Voluntariados)
                .HasForeignKey(d => d.UtilizadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKVoluntaria135401");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
