using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CommuniCare.Controllers;
using CommuniCare.DTOs;
using CommuniCare.Models;

namespace CommuniCareTests
{

    [TestClass]
    public class LojasControllerTests
    {
        private Mock<CommuniCareContext> _mockContext;
        private Mock<DbSet<Utilizador>> _mockUtilizadores;
        private Mock<DbSet<Loja>> _mockLojas;
        private LojasController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CommuniCareContext>();
            _mockUtilizadores = new Mock<DbSet<Utilizador>>();
            _mockLojas = new Mock<DbSet<Loja>>();

            _mockContext.Setup(c => c.Utilizadores).Returns(_mockUtilizadores.Object);
            _mockContext.Setup(c => c.Lojas).Returns(_mockLojas.Object);

            _controller = new LojasController(_mockContext.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }

            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

            public ValueTask DisposeAsync() => new ValueTask();

            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());

            public T Current => _inner.Current;
        }

        public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

            public IQueryable CreateQuery(Expression expression)
                => new TestAsyncEnumerable<TEntity>(expression); // agora funciona

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                => new TestAsyncEnumerable<TElement>(expression);

            public object Execute(Expression expression) => _inner.Execute(expression);

            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
                => Task.FromResult(Execute<TResult>(expression)).Result;
        }


        #region CriarLoja Tests
        [TestMethod]
        public async Task CriarLoja_DeveRetornarCreated_SeLojaCriadaComSucesso()
        {
            // Arrange
            var utilizador = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 2 };
            _mockUtilizadores.Setup(m => m.FindAsync(1)).ReturnsAsync(utilizador);

            var lojaAtiva = new Loja { LojaId = 1, NomeLoja = "Loja Ativa", Estado = EstadoLoja.Ativo };
            var lojasData = new List<Loja> { lojaAtiva }.AsQueryable();

            var mockSet = new Mock<DbSet<Loja>>();
            mockSet.As<IAsyncEnumerable<Loja>>()
                   .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                   .Returns(new TestAsyncEnumerator<Loja>(lojasData.GetEnumerator()));

            mockSet.As<IQueryable<Loja>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Loja>(lojasData.Provider));
            mockSet.As<IQueryable<Loja>>().Setup(m => m.Expression).Returns(lojasData.Expression);
            mockSet.As<IQueryable<Loja>>().Setup(m => m.ElementType).Returns(lojasData.ElementType);
            mockSet.As<IQueryable<Loja>>().Setup(m => m.GetEnumerator()).Returns(lojasData.GetEnumerator);

            var listaInterna = lojasData.ToList();
            mockSet.Setup(m => m.Add(It.IsAny<Loja>())).Callback<Loja>(l => listaInterna.Add(l));

            _mockContext.Setup(c => c.Set<Loja>()).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var lojaDto = new LojaDto { NomeLoja = "Nova Loja", DescLoja = "Descrição" };

            // Act
            var result = await _controller.CriarLoja(lojaDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
            var createdResult = result as CreatedAtActionResult;
            Assert.AreEqual("GetLoja", createdResult.ActionName);
            var createdLoja = createdResult.Value as dynamic;
            Assert.AreEqual("Nova Loja", createdLoja.nomeLoja);
            Assert.AreEqual(EstadoLoja.Ativo.ToString(), createdLoja.estado);

            mockSet.Verify(m => m.Add(It.IsAny<Loja>()), Times.Once());
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }



        [TestMethod]
        public async Task CriarLoja_DeveRetornarUnauthorized_SeUsuarioNaoAutenticado()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // Sem claims
            };

            var lojaDto = new LojaDto { NomeLoja = "Nova Loja", DescLoja = "Descrição" };

            // Act
            var result = await _controller.CriarLoja(lojaDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorizedResult.Value);
        }

        [TestMethod]
        public async Task CriarLoja_DeveRetornarForbid_SeUsuarioNaoAdmin()
        {
            // Arrange
            var utilizador = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 1 }; // Não admin
            _mockUtilizadores.Setup(m => m.FindAsync(1)).ReturnsAsync(utilizador);

            var lojaDto = new LojaDto { NomeLoja = "Nova Loja", DescLoja = "Descrição" };

            // Act
            var result = await _controller.CriarLoja(lojaDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        [TestMethod]
        public async Task CriarLoja_DeveRetornarForbid_SeUsuarioNaoExiste()
        {
            // Arrange
            _mockUtilizadores.Setup(m => m.FindAsync(1)).ReturnsAsync((Utilizador)null);

            var lojaDto = new LojaDto { NomeLoja = "Nova Loja", DescLoja = "Descrição" };

            // Act
            var result = await _controller.CriarLoja(lojaDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }
        #endregion

        #region AtivarLoja Tests
        [TestMethod]
        public async Task AtivarLoja_DeveRetornarOk_SeLojaAtivadaComSucesso()
        {
            // Arrange
            var utilizador = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 2 };
            _mockUtilizadores.Setup(m => m.FindAsync(1)).ReturnsAsync(utilizador);

            var lojaParaAtivar = new Loja { LojaId = 1, NomeLoja = "Loja 1", Estado = EstadoLoja.Inativo };
            var outraLoja = new Loja { LojaId = 2, NomeLoja = "Loja 2", Estado = EstadoLoja.Ativo };

            var lojasData = new List<Loja> { lojaParaAtivar, outraLoja }.AsQueryable();
            var mockLojasData = lojasData.ToList(); // precisa estar em lista para ser modificável

            _mockLojas.As<IQueryable<Loja>>().Setup(m => m.Provider).Returns(mockLojasData.AsQueryable().Provider);
            _mockLojas.As<IQueryable<Loja>>().Setup(m => m.Expression).Returns(mockLojasData.AsQueryable().Expression);
            _mockLojas.As<IQueryable<Loja>>().Setup(m => m.ElementType).Returns(mockLojasData.AsQueryable().ElementType);
            _mockLojas.As<IQueryable<Loja>>().Setup(m => m.GetEnumerator()).Returns(mockLojasData.GetEnumerator());
            _mockLojas.Setup(m => m.FindAsync(1)).ReturnsAsync(lojaParaAtivar);

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _controller.AtivarLoja(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;

            // Cast para objeto anônimo via reflexão
            var value = okResult.Value;
            var type = value.GetType();
            var mensagem = type.GetProperty("mensagem")?.GetValue(value)?.ToString();
            var lojaId = (int?)type.GetProperty("lojaId")?.GetValue(value);
            var estado = type.GetProperty("estado")?.GetValue(value)?.ToString();

            Assert.AreEqual("Loja ativada com sucesso.", mensagem);
            Assert.AreEqual(1, lojaId);
            Assert.AreEqual(EstadoLoja.Ativo.ToString(), estado);

            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }




        [TestMethod]
        public async Task AtivarLoja_DeveRetornarUnauthorized_SeUsuarioNaoAutenticado()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // Sem claims
            };

            // Act
            var result = await _controller.AtivarLoja(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorizedResult.Value);
        }

        [TestMethod]
        public async Task AtivarLoja_DeveRetornarForbid_SeUsuarioNaoAdmin()
        {
            // Arrange
            var utilizador = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 1 }; // Não admin
            _mockUtilizadores.Setup(m => m.FindAsync(1)).ReturnsAsync(utilizador);

            // Act
            var result = await _controller.AtivarLoja(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        [TestMethod]
        public async Task AtivarLoja_DeveRetornarNotFound_SeLojaNaoExiste()
        {
            // Arrange
            var utilizador = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 2 };
            _mockUtilizadores.Setup(m => m.FindAsync(1)).ReturnsAsync(utilizador);

            _mockLojas.Setup(m => m.FindAsync(1)).ReturnsAsync((Loja)null);

            // Act
            var result = await _controller.AtivarLoja(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Loja não encontrada.", notFoundResult.Value);
        }
        #endregion
    }

}