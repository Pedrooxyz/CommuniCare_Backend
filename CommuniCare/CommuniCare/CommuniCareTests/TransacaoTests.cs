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

        // Simulando as transações
        var transacoes = new List<Transacao>
        {
            new Transacao
            {
                TransacaoId = 1,
                Quantidade = 10,
                DataTransacao = DateTime.Now.AddMinutes(-10),
                TransacaoAjuda = new TransacaoAjuda { RecetorTran = utilizadorId }
            },
            new Transacao
            {
                TransacaoId = 2,
                Quantidade = 5,
                DataTransacao = DateTime.Now.AddMinutes(-5),
                TransacaoEmprestimo = new TransacaoEmprestimo { RecetorTran = utilizadorId }
            },
            new Transacao
            {
                TransacaoId = 3,
                Quantidade = 7,
                DataTransacao = DateTime.Now.AddMinutes(-15),
                Venda = new Venda { UtilizadorId = utilizadorId }
            }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Transacao>>();
        mockSet.As<IQueryable<Transacao>>().Setup(m => m.Provider).Returns(transacoes.Provider);
        mockSet.As<IQueryable<Transacao>>().Setup(m => m.Expression).Returns(transacoes.Expression);
        mockSet.As<IQueryable<Transacao>>().Setup(m => m.ElementType).Returns(transacoes.ElementType);
        mockSet.As<IQueryable<Transacao>>().Setup(m => m.GetEnumerator()).Returns(transacoes.GetEnumerator());

        // Simulando ToListAsync com Task.FromResult
        mockSet.Setup(m => m.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transacoes.ToList());

        _mockContext.Setup(c => c.Transacoes).Returns(mockSet.Object);

        // Act
        var result = await _controller.GetHistoricoTransacoes(utilizadorId);

        // Assert
        var actionResult = result as ActionResult<IEnumerable<object>>;
        Assert.IsNotNull(actionResult);
        var okResult = actionResult.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var historico = okResult.Value as List<object>;
        Assert.IsNotNull(historico);
        Assert.AreEqual(3, historico.Count);  // As 3 transações

        var transacao1 = historico[0] as dynamic;
        Assert.AreEqual("Ajuda", transacao1.Tipo);
        Assert.AreEqual("10", transacao1.NumeroCarenciasTransferido.ToString());

        var transacao2 = historico[1] as dynamic;
        Assert.AreEqual("Emprestimo", transacao2.Tipo);
        Assert.AreEqual("5", transacao2.NumeroCarenciasTransferido.ToString());

        var transacao3 = historico[2] as dynamic;
        Assert.AreEqual("Venda", transacao3.Tipo);
        Assert.AreEqual("7", transacao3.NumeroCarenciasTransferido.ToString());
    }

    [TestMethod]
    public async Task GetHistoricoTransacoes_DeveRetornarEmpty_SeNaoHouverTransacoes()
    {
        // Arrange
        var utilizadorId = 1;

        var transacoes = new List<Transacao>().AsQueryable();

        var mockSet = new Mock<DbSet<Transacao>>();
        mockSet.As<IQueryable<Transacao>>().Setup(m => m.Provider).Returns(transacoes.Provider);
        mockSet.As<IQueryable<Transacao>>().Setup(m => m.Expression).Returns(transacoes.Expression);
        mockSet.As<IQueryable<Transacao>>().Setup(m => m.ElementType).Returns(transacoes.ElementType);
        mockSet.As<IQueryable<Transacao>>().Setup(m => m.GetEnumerator()).Returns(transacoes.GetEnumerator());

        // Simulando ToListAsync com Task.FromResult
        mockSet.Setup(m => m.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transacoes.ToList());

        _mockContext.Setup(c => c.Transacoes).Returns(mockSet.Object);

        // Act
        var result = await _controller.GetHistoricoTransacoes(utilizadorId);

        // Assert
        var actionResult = result as ActionResult<IEnumerable<object>>;
        Assert.IsNotNull(actionResult);
        var okResult = actionResult.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var historico = okResult.Value as List<object>;
        Assert.IsNotNull(historico);
        Assert.AreEqual(0, historico.Count);  // Nenhuma transação
    }
    #endregion
}
