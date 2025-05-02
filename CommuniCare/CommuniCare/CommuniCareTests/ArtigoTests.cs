/// <summary>
/// Namespace que contém os testes unitários da aplicação CommuniCare.
/// Utiliza o framework MSTest e Moq para simular interações com a base de dados e testar o comportamento dos métodos do controlador LojasController.
/// </summary>

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
    /// <summary>
    /// Classe de testes para os métodos do controlador <see cref="ArtigosController"/>.
    /// Contém testes unitários que verificam o comportamento esperado dos métodos relacionados a artigos.
    /// </summary>
    [TestClass]
    public class ArtigosTests
    {
        private CommuniCareContext _context;
        private ArtigosController _controller;
        private Mock<DbSet<Artigo>> _mockArtigosDbSet;
        private Mock<DbSet<Loja>> _mockLojasDbSet;
        private Mock<DbSet<Utilizador>> _mockUtilizadoresDbSet;
        private Mock<CommuniCareContext> _mockContext;

        /// <summary>
        /// Inicializa o ambiente de teste antes da execução de cada método de teste.
        /// Cria mocks para os DbSet e para o contexto do banco de dados, e instancia o controller a ser testado.
        /// </summary>
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

        /// <summary>
        /// Testa se o método GetArtigosDisponiveis retorna um resultado Ok com a lista de artigos disponíveis quando existem artigos disponíveis na base de dados.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task GetArtigosDisponiveis_ReturnsOk_WhenArtigosExist()
        {

            var artigosList = new List<Artigo>
            {
                new Artigo { ArtigoId = 1, NomeArtigo = "Artigo 1", Estado = EstadoArtigo.Disponivel },
                new Artigo { ArtigoId = 2, NomeArtigo = "Artigo 2", Estado = EstadoArtigo.Disponivel }
            };

            var mockArtigosDbSet = artigosList.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.Artigos).Returns(mockArtigosDbSet.Object);


            var result = await _controller.GetArtigosDisponiveis();


            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(2, ((List<Artigo>)okResult.Value).Count);
        }

        #endregion

        #region PublicarArtigos

        /// <summary>
        /// Testa se o método PublicarArtigo retorna CreatedAtActionResult quando os dados fornecidos são válidos e o artigo é publicado com sucesso.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task PublicarArtigo_ValidData_ReturnsOk()
        {

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


            var result = await controller.PublicarArtigo(dto);


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

        /// <summary>
        /// Testa se o método IndisponibilizarArtigo retorna OkObjectResult com a mensagem apropriada quando um administrador marca um artigo como indisponível.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task IndisponibilizarArtigo_Admin_ReturnsOk()
        {

            var artigo = new Artigo { ArtigoId = 1, NomeArtigo = "Artigo Teste", Estado = EstadoArtigo.Disponivel };
            var utilizador = new Utilizador { UtilizadorId = 2, TipoUtilizadorId = 2, NomeUtilizador = "Admin" };

            _mockContext.Setup(c => c.Artigos.FindAsync(1)).ReturnsAsync(artigo);
            _mockContext.Setup(c => c.Utilizadores.FindAsync(2)).ReturnsAsync(utilizador);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);


            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "2") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };


            var result = await _controller.IndisponibilizarArtigo(1);


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Artigo indisponibilizado com sucesso.", okResult.Value);
        }

        #endregion

        #region ReporStock

        /// <summary>
        /// Classe de contexto de banco de dados usada para testes unitários com uma base de dados em memória.
        /// </summary>
        private sealed class TestContext : CommuniCareContext
        {
            public TestContext(DbContextOptions<CommuniCareContext> opts) : base(opts) { }
            protected override void OnConfiguring(DbContextOptionsBuilder _) { /* no-op */ }
        }

        /// <summary>
        /// Testa se o método ReporStock retorna OkObjectResult e atualiza corretamente a quantidade disponível do artigo
        /// quando os dados são válidos e o utilizador está autenticado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>
        [TestMethod]
        public async Task ReporStock_ReturnsOk_WhenSuccessful()
        {

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


            var result = await controller.ReporStock(artigoId, dto);


            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result.Result;
            var resposta = ok.Value as ArtigoRespostaDto;

            Assert.IsNotNull(resposta);
            Assert.AreEqual(artigoId, resposta.ArtigoId);
            Assert.AreEqual(7, resposta.QuantidadeDisponivel);

            var recarregado = await ctx.Artigos.FindAsync(artigoId);
            Assert.AreEqual(7, recarregado.QuantidadeDisponivel);
            Assert.AreEqual(EstadoArtigo.Disponivel, recarregado.Estado);
        }

        #endregion

        #endregion

        #region Testes de Erro

        #region GetArtigosDisponiveis

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetArtigosDisponiveis_ReturnsOk_WhenNoArtigosExist()
        {

            var artigosList = new List<Artigo>();

            var mockArtigosDbSet = artigosList.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.Artigos).Returns(mockArtigosDbSet.Object);


            var result = await _controller.GetArtigosDisponiveis();


            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(0, ((List<Artigo>)okResult.Value).Count);
        }

        #endregion

        #region PublicarArtigos

        /// <summary>
        /// Testa se o método PublicarArtigo retorna BadRequestObjectResult quando não existe nenhuma loja ativa no sistema.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>
        [TestMethod]
        public async Task PublicarArtigo_ReturnsBadRequest_WhenNoActiveLoja()
        {

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


            var result = await _controller.PublicarArtigo(dto);


            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result.Result;
            Assert.AreEqual("Não existe nenhuma loja ativa no momento.", bad.Value);
        }

        #endregion

        #region Indisponibilizar

        /// <summary>
        /// Testa se o método IndisponibilizarArtigo retorna ForbidResult quando o utilizador autenticado não é um administrador.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>
        [TestMethod]
        public async Task IndisponibilizarArtigo_ReturnsForbid_WhenUserIsNotAdmin()
        {

            int artigoId = 1;
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "1");
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { userIdClaim }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };


            var result = await _controller.IndisponibilizarArtigo(artigoId);


            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        #endregion

        #region ReporStock

        /// <summary>
        /// Testa se o método ReporStock retorna BadRequest quando a quantidade informada é inválida (menor ou igual a zero).
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ReporStock_ReturnsBadRequest_WhenQuantidadeInvalid()
        {

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


            var result = await _controller.ReporStock(artigoId, dto);


            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result.Result;
            Assert.AreEqual("A quantidade deve ser maior que zero.", bad.Value);
        }


        #endregion

        #endregion

    }
}