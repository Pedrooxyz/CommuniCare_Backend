using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommuniCare.Controllers;
using CommuniCare.Models;
using Microsoft.AspNetCore.Mvc;
using MockQueryable.Moq;

namespace CommuniCareTests
{

    [TestClass]
    public class TransacoesControllerTests
    {
        private Mock<CommuniCareContext> _mockContext;
        private Mock<DbSet<Transacao>> _mockTransacoes;
        private TransacoesController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CommuniCareContext>();
            _mockTransacoes = new Mock<DbSet<Transacao>>();
            _controller = new TransacoesController(_mockContext.Object);
        }

        #region GetHistoricoTransacoes Tests
        [TestMethod]
        public async Task GetHistoricoTransacoes_DeveRetornarHistoricoCorreto_SeExistiremTransacoes()
        {
            // Arrange
            var utilizadorId = 1;

            var now = DateTime.Now;

            var transacoes = new List<Transacao>
    {
        new Transacao
        {
            TransacaoId = 1,
            Quantidade = 10,
            DataTransacao = now.AddMinutes(-10),
            TransacaoAjuda = new TransacaoAjuda { RecetorTran = utilizadorId }
        },
        new Transacao
        {
            TransacaoId = 2,
            Quantidade = 5,
            DataTransacao = now.AddMinutes(-5),
            TransacaoEmprestimo = new TransacaoEmprestimo { RecetorTran = utilizadorId }
        },
        new Transacao
        {
            TransacaoId = 3,
            Quantidade = 7,
            DataTransacao = now.AddMinutes(-15),
            Venda = new Venda { UtilizadorId = utilizadorId }
        }
    }.AsQueryable();

            var mockSet = transacoes.BuildMockDbSet();

            _mockContext = new Mock<CommuniCareContext>();
            _mockContext.Setup(c => c.Transacoes).Returns(mockSet.Object);

            _controller = new TransacoesController(_mockContext.Object);

            // Act
            var result = await _controller.GetHistoricoTransacoes(utilizadorId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Result); 
            Assert.IsNotNull(result.Value);

            var historico = result.Value.ToList();
            Assert.AreEqual(3, historico.Count);

            var tipos = historico.Select(h => (string)h.GetType().GetProperty("Tipo")!.GetValue(h)).ToList();
            var quantidades = historico.Select(h => (int)h.GetType().GetProperty("NumeroCarenciasTransferido")!.GetValue(h)).ToList();

            CollectionAssert.Contains(tipos, "Ajuda");
            CollectionAssert.Contains(tipos, "Emprestimo");
            CollectionAssert.Contains(tipos, "Venda");

            CollectionAssert.Contains(quantidades, 10);
            CollectionAssert.Contains(quantidades, 5);
            CollectionAssert.Contains(quantidades, 7);
        }

        [TestMethod]
        public async Task GetHistoricoTransacoes_DeveRetornarEmpty_SeNaoHouverTransacoes()
        {
            // Arrange
            var utilizadorId = 1;
            var emptyTransacoes = new List<Transacao>().AsQueryable();

            var mockSet = emptyTransacoes.BuildMockDbSet(); 

            var mockContext = new Mock<CommuniCareContext>();
            mockContext.Setup(c => c.Transacoes).Returns(mockSet.Object);

            var controller = new TransacoesController(mockContext.Object);

            // Act
            var result = await controller.GetHistoricoTransacoes(utilizadorId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Result); 
            Assert.IsNotNull(result.Value);

            var historico = result.Value.ToList();
            Assert.AreEqual(0, historico.Count); 
        }
        #endregion
    }

}