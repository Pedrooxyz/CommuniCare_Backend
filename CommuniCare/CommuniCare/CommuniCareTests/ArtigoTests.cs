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

        #region Indisponibilizar

[TestMethod]
public async Task IndisponibilizarArtigo_Admin_ReturnsOk()
{
    // Arrange
    var artigo = new Artigo { ArtigoId = 1, NomeArtigo = "Artigo Teste", Estado = EstadoArtigo.Disponivel };
    var utilizador = new Utilizador { UtilizadorId = 2, TipoUtilizadorId = 2, NomeUtilizador = "Admin" };

    _mockContext.Setup(c => c.Artigos.FindAsync(1)).ReturnsAsync(artigo);
    _mockContext.Setup(c => c.Utilizadores.FindAsync(2)).ReturnsAsync(utilizador);
    _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

    // Simula utilizador autenticado
    var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "2") };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var principal = new ClaimsPrincipal(identity);
    _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };

    // Act
    var result = await _controller.IndisponibilizarArtigo(1);

    // Assert
    Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    var okResult = result as OkObjectResult;
    Assert.AreEqual("Artigo indisponibilizado com sucesso.", okResult.Value);
}

#endregion
        
        #region ReporStock

        [TestMethod]
        public async Task ReporStock_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var utilizadorId = 1;
            var artigoId = 10;
            var lojaAtiva = new Loja { LojaId = 5, Estado = EstadoLoja.Ativo };
            var artigo = new Artigo
            {
                ArtigoId = artigoId,
                NomeArtigo = "Artigo Teste",
                DescArtigo = "Descrição",
                CustoCares = 100,
                QuantidadeDisponivel = 2,
                LojaId = lojaAtiva.LojaId,
                Estado = EstadoArtigo.Disponivel
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            _mockContext.Setup(c => c.Utilizadores.FindAsync(utilizadorId))
                        .ReturnsAsync(new Utilizador { UtilizadorId = utilizadorId });
            _mockContext.Setup(c => c.Lojas.FirstOrDefaultAsync(It.IsAny<Expression<Func<Loja, bool>>>(), default))
                        .ReturnsAsync(lojaAtiva);
            _mockContext.Setup(c => c.Artigos.FindAsync(artigoId))
                        .ReturnsAsync(artigo);

            var dto = new ReporStockDto { Quantidade = 5 };

            // Act
            var result = await _controller.ReporStock(artigoId, dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            var resposta = okResult.Value as ArtigoRespostaDto;
            Assert.AreEqual(7, resposta.QuantidadeDisponivel); // 2 + 5
            Assert.AreEqual(artigoId, resposta.ArtigoId);
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

        #region Indisponibilizar

        [TestMethod]
        public async Task IndisponibilizarArtigo_ReturnsForbid_WhenUserIsNotAdmin()
        {
            // Arrange
            int artigoId = 1;
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "1");
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { userIdClaim }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.IndisponibilizarArtigo(artigoId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        #endregion
        
        #region ReporStock

        [TestMethod]
        public async Task ReporStock_ReturnsBadRequest_WhenQuantidadeInvalid()
        {
            // Arrange
            var utilizadorId = 1;
            var artigoId = 10;
            var lojaAtiva = new Loja { LojaId = 5, Estado = EstadoLoja.Ativo };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            _mockContext.Setup(c => c.Utilizadores.FindAsync(utilizadorId))
                        .ReturnsAsync(new Utilizador { UtilizadorId = utilizadorId });
            _mockContext.Setup(c => c.Lojas.FirstOrDefaultAsync(It.IsAny<Expression<Func<Loja, bool>>>(), default))
                        .ReturnsAsync(lojaAtiva);

            var dto = new ReporStockDto { Quantidade = 0 }; // Quantidade inválida

            // Act
            var result = await _controller.ReporStock(artigoId, dto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.AreEqual("A quantidade deve ser maior que zero.", badRequestResult.Value);
        }


        #endregion

        #endregion

    }
}