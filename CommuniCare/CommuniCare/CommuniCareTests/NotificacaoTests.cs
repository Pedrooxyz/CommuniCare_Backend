/// <summary>
/// Namespace que contém os testes unitários da aplicação CommuniCare.
/// Utiliza o framework MSTest e Moq para simular interações com a base de dados e testar o comportamento dos métodos do controlador VendaController.
/// </summary>
/// 

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
    /// Classe de testes para o controlador NotificacoesController.
    /// Contém métodos de teste para verificar o comportamento das notificações, incluindo cenários de sucesso e falha.
    /// </summary>
    [TestClass]
    public class NotificacoesControllerTests
    {
        private Mock<CommuniCareContext> _mockContext;
        private Mock<DbSet<Notificacao>> _mockNotificacoes;
        private NotificacoesController _controller;

        /// <summary>
        /// Método de inicialização que prepara o ambiente de testes.
        /// Configura os mocks do contexto de dados e do DbSet de notificações.
        /// Cria uma instância do controlador e configura o contexto do utilizador autenticado.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CommuniCareContext>();
            _mockNotificacoes = new Mock<DbSet<Notificacao>>();

            _mockContext.Setup(c => c.Notificacaos).Returns(_mockNotificacoes.Object);

            _controller = new NotificacoesController(_mockContext.Object);

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

        #region VerNotificacoes Tests

        /// <summary>
        /// Teste que verifica se o método VerNotificacoes retorna um resultado OK (200) quando existem notificações para o utilizador.
        /// </summary>
        /// <returns>Retorna um resultado OK com a lista de notificações.</returns>

        [TestMethod]
        public async Task VerNotificacoes_DeveRetornarOk_SeNotificacoesExistirem()
        {

            const int utilizadorId = 1;

            var dados = new[]
            {
                new Notificacao
                {
                    NotificacaoId = 1,
                    UtilizadorId  = utilizadorId,
                    Mensagem      = "Notificação 1",
                    Lida          = 0,
                    DataMensagem  = DateTime.Now.AddMinutes(-10)
                },
                new Notificacao
                {
                    NotificacaoId = 2,
                    UtilizadorId  = utilizadorId,
                    Mensagem      = "Notificação 2",
                    Lida          = 0,
                    DataMensagem  = DateTime.Now.AddMinutes(-5)
                }
            };


            var notificacoesDb = dados.AsQueryable().BuildMockDbSet();


            _mockContext.Setup(c => c.Notificacaos).Returns(notificacoesDb.Object);


            var result = await _controller.VerNotificacoes();


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            var lista = ok.Value as IEnumerable<Notificacao>;

            Assert.IsNotNull(lista);
            var notifs = lista.ToList();
            Assert.AreEqual(2, notifs.Count);
            Assert.IsTrue(notifs.All(n => n.UtilizadorId == utilizadorId));
        }

        /// <summary>
        /// Teste que verifica se o método VerNotificacoes retorna um resultado Unauthorized (401) quando o utilizador não está autenticado.
        /// </summary>
        /// <returns>Retorna um resultado Unauthorized com a mensagem de erro apropriada.</returns>
        [TestMethod]
        public async Task VerNotificacoes_DeveRetornarUnauthorized_SeUsuarioNaoAutenticado()
        {

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // Sem claims
            };


            var result = await _controller.VerNotificacoes();


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorizedResult.Value);
        }

        /// <summary>
        /// Teste que verifica se o método VerNotificacoes retorna um resultado NotFound (404) quando não existem notificações para o utilizador.
        /// </summary>
        /// <returns>Retorna um resultado NotFound com a mensagem "Não há notificações para mostrar." quando não existem notificações.</returns>
        [TestMethod]
        public async Task VerNotificacoes_DeveRetornarNotFound_SeNaoHouverNotificacoes()
        {

            const int userId = 5;


            var notificacoesDb = new List<Notificacao>()
                                 .AsQueryable()
                                 .BuildMockDbSet();

            _mockContext.Setup(c => c.Notificacaos).Returns(notificacoesDb.Object);


            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                             new ClaimsIdentity(
                               new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                               "TestAuth"))
                }
            };


            var resultado = await _controller.VerNotificacoes();


            Assert.IsInstanceOfType(resultado, typeof(NotFoundObjectResult));
            var nf = (NotFoundObjectResult)resultado;
            Assert.AreEqual("Não há notificações para mostrar.", nf.Value);


        }
        #endregion
    }
}