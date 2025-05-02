/// <summary>
/// Namespace que contém os testes unitários da aplicação CommuniCare.
/// Utiliza o framework MSTest e Moq para simular interações com a base de dados e testar o comportamento dos métodos do controlador LojasController.
/// </summary>

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
using MockQueryable.Moq;


namespace CommuniCareTests
{
    /// <summary>
    /// Classe de teste unitário para o controlador LojasController.
    /// Testa os métodos de criação, ativação e gerenciamento das lojas da aplicação CommuniCare.
    /// </summary>
    [TestClass]
    public class LojasControllerTests
    {
        private Mock<CommuniCareContext> _mockContext;
        private Mock<DbSet<Utilizador>> _mockUtilizadores;
        private Mock<DbSet<Loja>> _mockLojas;
        private LojasController _controller;
        private Mock<CommuniCareContext> _ctx;

        /// <summary>
        /// Configura os objetos necessários para os testes.
        /// Cria mocks para o contexto, utilizadores e lojas.
        /// Inicializa o controlador com o contexto simulado.
        /// </summary>
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
                => new TestAsyncEnumerable<TEntity>(expression);

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                => new TestAsyncEnumerable<TElement>(expression);

            public object Execute(Expression expression) => _inner.Execute(expression);

            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
                => Task.FromResult(Execute<TResult>(expression)).Result;
        }


        #region CriarLoja Tests

        /// <summary>
        /// Teste unitário para o método CriarLoja.
        /// Verifica se uma nova loja é criada com sucesso e se a resposta retorna o status 201 (Created).
        /// O teste simula a criação de uma nova loja no sistema e valida os valores retornados.
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task CriarLoja_DeveRetornarCreated_SeLojaCriadaComSucesso()
        {

            const int adminId = 1;


            var admin = new Utilizador { UtilizadorId = adminId, TipoUtilizadorId = 2 };
            var usersDbMock = new[] { admin }.AsQueryable().BuildMockDbSet();
            usersDbMock
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(admin);


            var lojaAtiva = new Loja { LojaId = 1, NomeLoja = "Loja Ativa", Estado = EstadoLoja.Ativo };
            var lojasLista = new List<Loja> { lojaAtiva };
            var lojasDbMock = lojasLista.AsQueryable().BuildMockDbSet();


            Loja novaLoja = null!;
            lojasDbMock
                .Setup(d => d.Add(It.IsAny<Loja>()))
                .Callback<Loja>(l => { novaLoja = l; lojasLista.Add(l); });


            var ctx = new Mock<CommuniCareContext>();
            ctx.Setup(c => c.Utilizadores).Returns(usersDbMock.Object);
            ctx.Setup(c => c.Lojas).Returns(lojasDbMock.Object);
            ctx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);


            var controller = new LojasController(ctx.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                                 new ClaimsIdentity(
                                   new[] { new Claim(ClaimTypes.NameIdentifier, adminId.ToString()) },
                                   "TestAuth"))
                    }
                }
            };

            var dto = new LojaDto { NomeLoja = "Nova Loja", DescLoja = "Descrição" };


            var result = await controller.CriarLoja(dto);


            Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
            var created = (CreatedAtActionResult)result;


            var payload = created.Value!;
            var t = payload.GetType();
            var nome = t.GetProperty("nomeLoja")!.GetValue(payload);
            var estado = t.GetProperty("estado")!.GetValue(payload);

            Assert.AreEqual("Nova Loja", nome);
            Assert.AreEqual(EstadoLoja.Ativo.ToString(), estado);


            Assert.AreEqual(EstadoLoja.Inativo, lojaAtiva.Estado);
            Assert.IsNotNull(novaLoja);
            Assert.AreEqual(EstadoLoja.Ativo, novaLoja.Estado);


            lojasDbMock.Verify(d => d.Add(It.IsAny<Loja>()), Times.Once);
            ctx.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        /// <summary>
        /// Teste unitário para o método CriarLoja.
        /// Verifica se o método retorna Unauthorized (401) quando o usuário não está autenticado.
        /// Este teste simula um cenário onde o controlador não tem um usuário autenticado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>
        [TestMethod]
        public async Task CriarLoja_DeveRetornarUnauthorized_SeUsuarioNaoAutenticado()
        {

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // Sem claims
            };

            var lojaDto = new LojaDto { NomeLoja = "Nova Loja", DescLoja = "Descrição" };


            var result = await _controller.CriarLoja(lojaDto);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorizedResult.Value);
        }

        /// <summary>
        /// Teste unitário para o método CriarLoja.
        /// Verifica se o método retorna Forbid (403) quando o usuário não é um administrador.
        /// Este teste simula um cenário onde o usuário não possui permissões de administrador para criar uma loja.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário</returns>
        [TestMethod]
        public async Task CriarLoja_DeveRetornarForbid_SeUsuarioNaoAdmin()
        {

            var utilizador = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 1 };
            _mockUtilizadores.Setup(m => m.FindAsync(1)).ReturnsAsync(utilizador);

            var lojaDto = new LojaDto { NomeLoja = "Nova Loja", DescLoja = "Descrição" };


            var result = await _controller.CriarLoja(lojaDto);


            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        /// <summary>
        /// Teste unitário para o método CriarLoja.
        /// Verifica se o método retorna Forbid (403) quando o usuário não existe no sistema.
        /// Este teste simula um cenário onde o usuário não está cadastrado no banco de dados.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário</returns>
        [TestMethod]
        public async Task CriarLoja_DeveRetornarForbid_SeUsuarioNaoExiste()
        {

            _mockUtilizadores.Setup(m => m.FindAsync(1)).ReturnsAsync((Utilizador)null);

            var lojaDto = new LojaDto { NomeLoja = "Nova Loja", DescLoja = "Descrição" };


            var result = await _controller.CriarLoja(lojaDto);


            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }
        #endregion

        #region AtivarLoja Tests

        /// <summary>
        /// Teste unitário para o método AtivarLoja.
        /// Verifica se o método retorna Ok (200) e ativa a loja corretamente.
        /// Este teste simula a ativação de uma loja e valida se a operação foi realizada com sucesso.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário</returns>
        [TestMethod]
        public async Task AtivarLoja_DeveRetornarOk_SeLojaAtivadaComSucesso()
        {

            const int adminId = 1;
            const int lojaId1 = 1;
            const int lojaId2 = 2;


            var admin = new Utilizador { UtilizadorId = adminId, TipoUtilizadorId = 2 };

            var utilizadoresDb = new[] { admin }
                                 .AsQueryable()
                                 .BuildMockDbSet();

            utilizadoresDb
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(admin);

            _mockContext.Setup(c => c.Utilizadores).Returns(utilizadoresDb.Object);


            var lojaParaAtivar = new Loja { LojaId = lojaId1, NomeLoja = "Loja 1", Estado = EstadoLoja.Inativo };
            var outraLoja = new Loja { LojaId = lojaId2, NomeLoja = "Loja 2", Estado = EstadoLoja.Ativo };

            var lojasDb = new[] { lojaParaAtivar, outraLoja }
                          .AsQueryable()
                          .BuildMockDbSet();

            lojasDb
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(lojaParaAtivar);

            _mockContext.Setup(c => c.Lojas).Returns(lojasDb.Object);

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);


            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                             new ClaimsIdentity(
                               new[] { new Claim(ClaimTypes.NameIdentifier, adminId.ToString()) },
                               "TestAuth"))
                }
            };


            var result = await _controller.AtivarLoja(lojaId1);


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;


            var value = ok.Value;
            var t = value.GetType();
            var msg = t.GetProperty("mensagem")?.GetValue(value)?.ToString();
            var id = (int?)t.GetProperty("lojaId")?.GetValue(value);
            var estado = t.GetProperty("estado")?.GetValue(value)?.ToString();

            Assert.AreEqual("Loja ativada com sucesso.", msg);
            Assert.AreEqual(lojaId1, id);
            Assert.AreEqual(EstadoLoja.Ativo.ToString(), estado);


            Assert.AreEqual(EstadoLoja.Ativo, lojaParaAtivar.Estado);
            Assert.AreEqual(EstadoLoja.Inativo, outraLoja.Estado);

            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        /// <summary>
        /// Teste unitário para o método AtivarLoja.
        /// Verifica se o método retorna Unauthorized (401) quando o utilizador não está autenticado.
        /// Este teste simula um cenário em que o utilizador não está autenticado ao tentar ativar uma loja e valida se o retorno é o esperado (Unauthorized).
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário</returns>

        [TestMethod]
        public async Task AtivarLoja_DeveRetornarUnauthorized_SeUsuarioNaoAutenticado()
        {

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };


            var result = await _controller.AtivarLoja(1);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorizedResult.Value);
        }

        /// <summary>
        /// Teste unitário para o método AtivarLoja.
        /// Verifica se o método retorna Forbid (403) quando o utilizador não tem permissões de administrador.
        /// Este teste simula um cenário onde o utilizador não é um administrador (TipoUtilizadorId diferente de 2) 
        /// e valida se o método retorna o código de status adequado (Forbid) quando tenta ativar uma loja.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário</returns>
        [TestMethod]
        public async Task AtivarLoja_DeveRetornarForbid_SeUsuarioNaoAdmin()
        {

            var utilizador = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 1 };
            _mockUtilizadores.Setup(m => m.FindAsync(1)).ReturnsAsync(utilizador);


            var result = await _controller.AtivarLoja(1);


            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        /// <summary>
        /// Teste unitário para o método AtivarLoja.
        /// Verifica se o método retorna NotFound (404) quando a loja não existe no banco de dados.
        /// Este teste simula um cenário onde uma loja não é encontrada e valida se o método retorna o código de status adequado (NotFound) com a mensagem de erro.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário</returns>
        [TestMethod]
        public async Task AtivarLoja_DeveRetornarNotFound_SeLojaNaoExiste()
        {

            var utilizador = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 2 };
            _mockUtilizadores.Setup(m => m.FindAsync(1)).ReturnsAsync(utilizador);

            _mockLojas.Setup(m => m.FindAsync(1)).ReturnsAsync((Loja)null);


            var result = await _controller.AtivarLoja(1);


            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Loja não encontrada.", notFoundResult.Value);
        }
        #endregion
    }

}