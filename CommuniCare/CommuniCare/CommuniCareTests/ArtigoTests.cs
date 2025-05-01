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


namespace CommuniCareTests
{
    [TestClass]
    public class ArtigosTests
    {
        private ArtigosController _controller;
        private Mock<DbSet<Artigo>> _mockArtigosDbSet;
        private Mock<DbSet<Loja>> _mockLojasDbSet;
        private Mock<DbSet<Utilizador>> _mockUtilizadoresDbSet;
        private Mock<CommuniCareContext> _mockContext;

        [TestInitialize]
        public void Setup()
        {
            _mockArtigosDbSet = new Mock<DbSet<Artigo>>();
            _mockLojasDbSet = new Mock<DbSet<Loja>>();
            _mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();

            _mockContext = new Mock<CommuniCareContext>();

            _mockContext.Setup(m => m.Artigos).Returns(_mockArtigosDbSet.Object);
            _mockContext.Setup(m => m.Lojas).Returns(_mockLojasDbSet.Object);
            _mockContext.Setup(m => m.Utilizadores).Returns(_mockUtilizadoresDbSet.Object);

            _controller = new ArtigosController(_mockContext.Object);
        }

        #region Testes de Sucesso

        #region GetArtigosDisponiveis

        [TestMethod]
        public async Task GetArtigosDisponiveis_ReturnsOk_WhenArtigosExist()
        {
            // Arrange
            var artigosList = new List<Artigo>
            {
                new Artigo { ArtigoId = 1, NomeArtigo = "Artigo 1", Estado = EstadoArtigo.Disponivel },
                new Artigo { ArtigoId = 2, NomeArtigo = "Artigo 2", Estado = EstadoArtigo.Disponivel }
            };

            var mockArtigosDbSet = artigosList.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.Artigos).Returns(mockArtigosDbSet.Object);

            // Act
            var result = await _controller.GetArtigosDisponiveis();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(2, ((List<Artigo>)okResult.Value).Count);
        }

        #endregion

        #region PublicarArtigos

        [TestMethod]
        public async Task PublicarArtigo_ValidData_ReturnsOk()
        {
            // Arrange
            var ArtigoData = new ArtigoDto
            {
                NomeArtigo = "Artigo Teste",
                DescArtigo = "Descrição do artigo",
                CustoCares = 100,
                QuantidadeDisponivel = 10,
                FotografiaArt = "foto.jpg"
            };

            var admin = new Utilizador
            {
                UtilizadorId = 2,
                TipoUtilizadorId = 2,
                NomeUtilizador = "Admin"
            };

            var utilizadoresList = new List<Utilizador> { admin }.AsQueryable();
            var mockUtilizadoresDbSet = utilizadoresList.BuildMockDbSet();

            var mockArtigosDbSet = new Mock<DbSet<Artigo>>();

            _mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresDbSet.Object);
            _mockContext.Setup(c => c.Artigos).Returns(mockArtigosDbSet.Object);

            _mockContext.Setup(c => c.Utilizadores.FindAsync(1)).ReturnsAsync(admin);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Simula utilizador autenticado
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "2")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.PublicarArtigo(ArtigoData);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            //var okResult = result as OkObjectResult;

            //// Mensagem corrigida para publicação de artigo
            //Assert.AreEqual("Artigo publicado com sucesso.", okResult.Value);
        }

        #endregion

        #endregion

        #region Testes de Erro

        #region GetArtigosDisponiveis

        [TestMethod]
        public async Task GetArtigosDisponiveis_ReturnsOk_WhenNoArtigosExist()
        {
            // Arrange
            var artigosList = new List<Artigo>(); // Lista vazia

            var mockArtigosDbSet = artigosList.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.Artigos).Returns(mockArtigosDbSet.Object);

            // Act
            var result = await _controller.GetArtigosDisponiveis();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(0, ((List<Artigo>)okResult.Value).Count);
        }

        #endregion

        #region PublicarArtigos

[TestMethod]
public async Task PublicarArtigo_ReturnsBadRequest_WhenNoActiveLoja()
{
    // Arrange
    var utilizadorId = 1;
    var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
    {
    new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString())
    }, "mock"));

    _controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext { User = user }
    };

    var utilizador = new Utilizador { UtilizadorId = utilizadorId };

    _mockContext.Setup(c => c.Utilizadores.FindAsync(utilizadorId))
                .ReturnsAsync(utilizador);
    _mockContext.Setup(c => c.Lojas.FirstOrDefaultAsync(It.IsAny<Expression<Func<Loja, bool>>>(), default))
                .ReturnsAsync((Loja)null); // Nenhuma loja ativa

    var dto = new ArtigoDto
    {
        NomeArtigo = "Novo Artigo",
        DescArtigo = "Descrição",
        CustoCares = 100,
        QuantidadeDisponivel = 5,
        FotografiaArt = "foto.jpg"
    };

    // Act
    var result = await _controller.PublicarArtigo(dto);

    // Assert
    Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    var badRequestResult = result.Result as BadRequestObjectResult;
    Assert.AreEqual("Não existe nenhuma loja ativa no momento.", badRequestResult.Value);
}

#endregion

        #endregion

    }
}