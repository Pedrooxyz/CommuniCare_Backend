using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommuniCare.Controllers;
using CommuniCare.Models;
using CommuniCare.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using MockQueryable.Moq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MockQueryable.EntityFrameworkCore;
using CommuniCare;


namespace CommuniCareTests
{
    [TestClass]
    public class VendaTests
    {
        private VendasController _controller;
        private Mock<DbSet<Venda>> _mockVendasDbSet;
        private Mock<DbSet<Transacao>> _mockTransacoesDbSet;
        private Mock<DbSet<Utilizador>> _mockUtilizadoresDbSet;
        private Mock<DbSet<Loja>> _mockLojasDbSet;
        private Mock<CommuniCareContext> _mockContext;
        private Mock<EmailService> _mockEmailService;
        private Mock<TransacaoServico> _mockTransacaoServico;

        [TestInitialize]
        public void Setup()
        {
            // Mock dos DbSet para os modelos Venda, Transacao, Utilizador e Loja
            _mockVendasDbSet = new Mock<DbSet<Venda>>();
            _mockTransacoesDbSet = new Mock<DbSet<Transacao>>();
            _mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();
            _mockLojasDbSet = new Mock<DbSet<Loja>>();

            // Mock do contexto de dados
            _mockContext = new Mock<CommuniCareContext>();

            // Configurar o retorno dos DbSet para o contexto
            _mockContext.Setup(m => m.Venda).Returns(_mockVendasDbSet.Object);
            _mockContext.Setup(m => m.Transacoes).Returns(_mockTransacoesDbSet.Object);
            _mockContext.Setup(m => m.Utilizadores).Returns(_mockUtilizadoresDbSet.Object);
            _mockContext.Setup(m => m.Lojas).Returns(_mockLojasDbSet.Object);

            // Mock do serviço de Email
            _mockEmailService = new Mock<EmailService>();

            // Mock do serviço de Transações
            _mockTransacaoServico = new Mock<TransacaoServico>(_mockContext.Object);

            // Criar a instância do controlador, passando o contexto mockado, o serviço de email e o serviço de transações
            _controller = new VendasController(_mockContext.Object, _mockEmailService.Object);
        }

        #region Testes de Sucesso

        #endregion

        #region Testes de Erro

        #endregion

    }
}
