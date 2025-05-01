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
using SendGrid.Helpers.Mail;


namespace CommuniCareTests
{
    [TestClass]
    public class ArtigosTests
    {
        private CommuniCareContext _context;
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
            /*  Arrange  */
            var dto = new ArtigoDto
            {
                NomeArtigo = "Artigo Teste",
                DescArtigo = "Desc",
                CustoCares = 100,
                QuantidadeDisponivel = 10,
                FotografiaArt = "foto.jpg"
            };

            
            var user = new Utilizador { UtilizadorId = 2, NomeUtilizador = "Admin" };

            var utilizadoresDb = new[] { user }
                                 .AsQueryable()
                                 .BuildMockDbSet();

            
            utilizadoresDb
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(user);

            
            var lojaAtiva = new Loja { LojaId = 10, Estado = EstadoLoja.Ativo };

            var lojasDb = new[] { lojaAtiva }
                          .AsQueryable()
                          .BuildMockDbSet();

            
            var artigosDb = new Mock<DbSet<Artigo>>();

            
            var ctx = new Mock<CommuniCareContext>();
            ctx.Setup(c => c.Utilizadores).Returns(utilizadoresDb.Object);
            ctx.Setup(c => c.Lojas).Returns(lojasDb.Object);
            ctx.Setup(c => c.Artigos).Returns(artigosDb.Object);
            ctx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

            
            var controller = new ArtigosController(ctx.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                                 new ClaimsIdentity(
                                   new[] { new Claim(ClaimTypes.NameIdentifier, "2") },
                                   "TestAuth"))
                    }
                }
            };

            /*  Act  */
            var result = await controller.PublicarArtigo(dto);

            /*  Assert  */
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));

            var created = (CreatedAtActionResult)result.Result;
            var resposta = created.Value as ArtigoRespostaDto;
            Assert.IsNotNull(resposta);
            Assert.AreEqual(dto.NomeArtigo, resposta.NomeArtigo);
            Assert.AreEqual(dto.DescArtigo, resposta.DescArtigo);
            Assert.AreEqual(dto.CustoCares, resposta.CustoCares);
            Assert.AreEqual(dto.QuantidadeDisponivel, resposta.QuantidadeDisponivel);

            
            artigosDb.Verify(d => d.Add(It.IsAny<Artigo>()), Times.Once);
            ctx.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        private sealed class TestContext : CommuniCareContext
        {
            public TestContext(DbContextOptions<CommuniCareContext> opts) : base(opts) { }
            protected override void OnConfiguring(DbContextOptionsBuilder _) { /* no-op */ }
        }

        [TestMethod]
        public async Task ReporStock_ReturnsOk_WhenSuccessful()
        {
            /*  Arrange  */
            const int userId = 1;
            const int lojaId = 5;
            const int artigoId = 10;

            var opts = new DbContextOptionsBuilder<CommuniCareContext>()
                           .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                           .Options;

            await using var ctx = new TestContext(opts);

            
            ctx.Utilizadores.Add(new Utilizador { UtilizadorId = userId });
            ctx.Lojas.Add(new Loja { LojaId = lojaId, Estado = EstadoLoja.Ativo });
            ctx.Artigos.Add(new Artigo
            {
                ArtigoId = artigoId,
                NomeArtigo = "Artigo Teste",
                DescArtigo = "Desc",
                CustoCares = 100,
                QuantidadeDisponivel = 2,
                LojaId = lojaId,
                Estado = EstadoArtigo.Disponivel
            });
            await ctx.SaveChangesAsync();

            var controller = new ArtigosController(ctx)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                                   new ClaimsIdentity(
                                       new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                                       "TestAuth"))
                    }
                }
            };

            var dto = new ReporStockDto { Quantidade = 5 };

            /*  Act  */
            var result = await controller.ReporStock(artigoId, dto);

            /*  Assert  */
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result.Result;
            var resposta = ok.Value as ArtigoRespostaDto;

            Assert.IsNotNull(resposta);
            Assert.AreEqual(artigoId, resposta.ArtigoId);
            Assert.AreEqual(7, resposta.QuantidadeDisponivel);   // 2 + 5

            var recarregado = await ctx.Artigos.FindAsync(artigoId);
            Assert.AreEqual(7, recarregado.QuantidadeDisponivel);
            Assert.AreEqual(EstadoArtigo.Disponivel, recarregado.Estado);
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
            /*  Arrange  */
            const int utilizadorId = 1;

            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                             new ClaimsIdentity(
                               new[] { new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString()) },
                               "TestAuth"))
                }
            };

           
            var utilizador = new Utilizador { UtilizadorId = utilizadorId };
            var utilizadoresDb = new[] { utilizador }
                                 .AsQueryable()
                                 .BuildMockDbSet();

            utilizadoresDb
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(utilizador);

            _mockContext.Setup(c => c.Utilizadores).Returns(utilizadoresDb.Object);

            
            var lojasDb = new List<Loja>()            
                          .AsQueryable()
                          .BuildMockDbSet();
            _mockContext.Setup(c => c.Lojas).Returns(lojasDb.Object);

            
            var dto = new ArtigoDto
            {
                NomeArtigo = "Novo Artigo",
                DescArtigo = "Descrição",
                CustoCares = 100,
                QuantidadeDisponivel = 5,
                FotografiaArt = "foto.jpg"
            };

            /*  Act  */
            var result = await _controller.PublicarArtigo(dto);

            /*  Assert  */
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result.Result;
            Assert.AreEqual("Não existe nenhuma loja ativa no momento.", bad.Value);
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
            /*  Arrange  */
            const int utilizadorId = 1;
            const int artigoId = 10;

            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                             new ClaimsIdentity(
                               new[] { new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString()) },
                               "TestAuth"))
                }
            };

           
            var utilizador = new Utilizador { UtilizadorId = utilizadorId };

            var utilizadoresDb = new[] { utilizador }
                                 .AsQueryable()
                                 .BuildMockDbSet();

            utilizadoresDb
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(utilizador);

            _mockContext.Setup(c => c.Utilizadores).Returns(utilizadoresDb.Object);

           
            var lojaAtiva = new Loja { LojaId = 5, Estado = EstadoLoja.Ativo };

            var lojasDb = new[] { lojaAtiva }
                          .AsQueryable()
                          .BuildMockDbSet();

            _mockContext.Setup(c => c.Lojas).Returns(lojasDb.Object);

            
            var dto = new ReporStockDto { Quantidade = 0 };

            /*  Act  */
            var result = await _controller.ReporStock(artigoId, dto);

            /*  Assert  */
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result.Result;
            Assert.AreEqual("A quantidade deve ser maior que zero.", bad.Value);
        }


        #endregion

        #endregion

    }
}