/// <summary>
/// Namespace que contém os testes unitários da aplicação CommuniCare.
/// Utiliza o framework MSTest e Moq para simular interações com a base de dados e testar o comportamento dos métodos do controlador VoluntariadoController.
/// </summary>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CommuniCare.Controllers;
using CommuniCare.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using MockQueryable.Moq;
using Microsoft.EntityFrameworkCore.Query;
using System;


namespace CommuniCareTests
{
    /// <summary>
    /// Classe de testes para o controlador de Voluntariado, incluindo cenários como rejeição de voluntários e outras funcionalidades associadas.
    /// </summary>

    [TestClass]
    public class VoluntariadoControllerTests
    {
        private Mock<CommuniCareContext> _mockContext;
        private Mock<DbSet<Artigo>> _mockArtigos;
        private Mock<DbSet<Favoritos>> _mockFavoritos;
        private FavoritosController _controller;

        /// <summary>
        /// Inicializa os recursos necessários para os testes.
        /// Este método configura o contexto mockado, os DbSets e o controlador.
        /// Também configura o usuário autenticado no contexto do controlador.
        /// </summary>

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CommuniCareContext>();
            _mockArtigos = new Mock<DbSet<Artigo>>();
            _mockFavoritos = new Mock<DbSet<Favoritos>>();

            _mockContext.Setup(c => c.Artigos).Returns(_mockArtigos.Object);
            _mockContext.Setup(c => c.Favoritos).Returns(_mockFavoritos.Object);

            _controller = new FavoritosController(_mockContext.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        #region RejeitarVoluntario

        /// <summary>
        /// Testa o comportamento do método RejeitarVoluntario quando um voluntário válido é rejeitado por um administrador.
        /// Espera-se que o método retorne um resultado Ok com uma mensagem de sucesso, que o voluntário seja removido e que uma notificação seja criada.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task RejeitarVoluntario_DeveRetornarOk_SeVoluntarioForRejeitado()
        {
            // Arrange
            var voluntariado = new Voluntariado
            {
                IdVoluntariado = 1,
                Estado = EstadoVoluntariado.Pendente,
                PedidoId = 1,
                UtilizadorId = 10,
                Pedido = new PedidoAjuda
                {
                    PedidoId = 1,
                    UtilizadorId = 5,
                    NPessoas = 2,
                    Voluntariados = new List<Voluntariado>()
                },
                Utilizador = new Utilizador { UtilizadorId = 10 }
            };

            var voluntariados = new List<Voluntariado> { voluntariado }.AsQueryable().BuildMockDbSet();
            voluntariados.Setup(m => m.Remove(It.IsAny<Voluntariado>())).Callback<Voluntariado>(v => { });

            var admin = new Utilizador { UtilizadorId = 10, TipoUtilizadorId = 2 };
            var mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();
            mockUtilizadoresDbSet.Setup(m => m.FindAsync(10)).ReturnsAsync(admin);

            var notificacaos = new List<Notificacao>();
            var notificacaosMock = new Mock<DbSet<Notificacao>>();
            notificacaosMock.Setup(m => m.Add(It.IsAny<Notificacao>())).Callback<Notificacao>(n => notificacaos.Add(n));

            var mockContext = new Mock<CommuniCareContext>();
            mockContext.Setup(c => c.Voluntariados).Returns(voluntariados.Object);
            mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresDbSet.Object);
            mockContext.Setup(c => c.Notificacaos).Returns(notificacaosMock.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var controller = new VoluntariadosController(mockContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, "10")
            }, "mock"))
                }
            };

            // Act
            var result = await controller.RejeitarVoluntario(1, 10);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Voluntário rejeitado com sucesso.", okResult.Value);

            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            notificacaosMock.Verify(m => m.Add(It.IsAny<Notificacao>()), Times.Exactly(2)); 

          
            Assert.AreEqual(2, notificacaos.Count);
            Assert.IsTrue(notificacaos.Any(n =>
                n.UtilizadorId == 10 &&
                n.Mensagem.Contains("foi rejeitada")
            ));
            Assert.IsTrue(notificacaos.Any(n =>
                n.UtilizadorId == 5 &&
                n.Mensagem.Contains("foi rejeitado")
            ));
        }


        /// <summary>
        /// Testa o comportamento do método RejeitarVoluntario quando o utilizador não está autenticado.
        /// Espera-se que o método retorne um resultado Unauthorized com uma mensagem informando que o utilizador não está autenticado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task RejeitarVoluntario_DeveRetornarUnauthorized_SeUsuarioNaoAutenticado()
        {

            var mockContext = new Mock<CommuniCareContext>();
            var controller = new VoluntariadosController(mockContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };


            var result = await controller.RejeitarVoluntario(1, 10);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorizedResult.Value);
        }


        /// <summary>
        /// Testa o comportamento do método RejeitarVoluntario quando o utilizador autenticado não é um administrador.
        /// Espera-se que o método retorne um resultado Forbid, indicando que o utilizador não tem permissões suficientes para realizar a ação.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task RejeitarVoluntario_DeveRetornarForbid_SeUsuarioNaoForAdmin()
        {

            var utilizador = new Utilizador { UtilizadorId = 10, TipoUtilizadorId = 1 };
            var mockUtilizadoresDbSet = new List<Utilizador> { utilizador }.AsQueryable().BuildMockDbSet();

            var mockContext = new Mock<CommuniCareContext>();
            mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresDbSet.Object);
            mockContext.Setup(c => c.Utilizadores.FindAsync(10)).ReturnsAsync(utilizador);

            var controller = new VoluntariadosController(mockContext.Object);
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "10") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };


            var result = await controller.RejeitarVoluntario(1, 10);


            Assert.IsInstanceOfType(result, typeof(ForbidResult));
            var forbidResult = result as ForbidResult;
            Assert.IsNotNull(forbidResult);
        }

        /// <summary>
        /// Testa o comportamento do método RejeitarVoluntario quando o ID do utilizador autenticado não corresponde a um utilizador existente na base de dados.
        /// Espera-se que o método retorne um resultado Forbid, indicando que o utilizador não tem autorização para executar a ação.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task RejeitarVoluntario_DeveRetornarForbid_SeAdminNaoExiste()
        {

            var mockUtilizadoresDbSet = new List<Utilizador>().AsQueryable().BuildMockDbSet();
            var mockContext = new Mock<CommuniCareContext>();
            mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresDbSet.Object);
            mockContext.Setup(c => c.Utilizadores.FindAsync(10)).ReturnsAsync((Utilizador)null);

            var controller = new VoluntariadosController(mockContext.Object);
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "10") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };


            var result = await controller.RejeitarVoluntario(1, 10);


            Assert.IsInstanceOfType(result, typeof(ForbidResult));
            var forbidResult = result as ForbidResult;
            Assert.IsNotNull(forbidResult);
        }


        /// <summary>
        /// Testa o comportamento do método RejeitarVoluntario quando o voluntariado especificado não existe ou já foi processado.
        /// Espera-se que o método retorne um NotFound (404) com uma mensagem indicando que o voluntariado não foi encontrado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task RejeitarVoluntario_DeveRetornarNotFound_SeVoluntariadoNaoExiste()
        {

            var data = new List<Voluntariado>().AsQueryable();
            var mockVoluntariadoDbSet = new Mock<DbSet<Voluntariado>>();
            var asyncQueryProvider = new Mock<IAsyncQueryProvider>();
            asyncQueryProvider
                .Setup(p => p.ExecuteAsync<Voluntariado>(It.IsAny<Expression>(), It.IsAny<CancellationToken>()))
                .Returns((Expression expression, CancellationToken _) =>
                {
                    var query = data.AsQueryable();
                    var filteredData = query.Where(v => v.IdVoluntariado == 1 && v.Estado == EstadoVoluntariado.Pendente);
                    return filteredData.FirstOrDefault();
                });
            asyncQueryProvider
                .Setup(p => p.ExecuteAsync<IEnumerable<Voluntariado>>(It.IsAny<Expression>(), It.IsAny<CancellationToken>()))
                .Returns((Expression expression, CancellationToken _) => data.Provider.Execute<IEnumerable<Voluntariado>>(expression));

            mockVoluntariadoDbSet.As<IQueryable<Voluntariado>>().Setup(m => m.Provider).Returns(asyncQueryProvider.Object);
            mockVoluntariadoDbSet.As<IQueryable<Voluntariado>>().Setup(m => m.Expression).Returns(data.Expression);
            mockVoluntariadoDbSet.As<IQueryable<Voluntariado>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockVoluntariadoDbSet.As<IQueryable<Voluntariado>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var utilizador = new Utilizador { UtilizadorId = 10, TipoUtilizadorId = 2 };
            var mockUtilizadoresDbSet = new List<Utilizador> { utilizador }.AsQueryable().BuildMockDbSet();

            var mockContext = new Mock<CommuniCareContext>();
            mockContext.Setup(c => c.Voluntariados).Returns(mockVoluntariadoDbSet.Object);
            mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresDbSet.Object);
            mockContext.Setup(c => c.Utilizadores.FindAsync(10)).ReturnsAsync(utilizador);

            var controller = new VoluntariadosController(mockContext.Object);
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "10") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };


            var result = await controller.RejeitarVoluntario(1, 10);


            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Voluntariado não encontrado ou já foi processado.", notFoundResult.Value);
        }

        #endregion

        #region AceitarVoluntario

        /// <summary>
        /// Testa se o método AceitarVoluntario retorna Ok quando um voluntário é aceito com sucesso
        /// e o estado do pedido de ajuda é atualizado para "Em Progresso".
        /// Também verifica se as notificações são criadas corretamente e se as alterações são persistidas.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task AceitarVoluntario_DeveRetornarOk_SeVoluntarioAceitoEPedidoEmProgresso()
        {

            var pedido = new PedidoAjuda
            {
                PedidoId = 1,
                UtilizadorId = 5,
                NPessoas = 2,
                Estado = EstadoPedido.Pendente,
                Voluntariados = new List<Voluntariado>
            {
                new Voluntariado { IdVoluntariado = 2, Estado = EstadoVoluntariado.Aceite }
                }
            };

            var voluntariado = new Voluntariado
            {
                IdVoluntariado = 1,
                Estado = EstadoVoluntariado.Pendente,
                PedidoId = 1,
                UtilizadorId = 10,
                Pedido = pedido,
                Utilizador = new Utilizador { UtilizadorId = 10 }
            };
            pedido.Voluntariados.Add(voluntariado);


            var voluntariados = new List<Voluntariado> { voluntariado }.AsQueryable().BuildMockDbSet();


            var adminUser = new Utilizador { UtilizadorId = 10, TipoUtilizadorId = 2 };
            var mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();
            mockUtilizadoresDbSet.Setup(m => m.FindAsync(10)).ReturnsAsync(adminUser);


            var notificacaos = new List<Notificacao>();
            var notificacaosMock = new Mock<DbSet<Notificacao>>();
            notificacaosMock.Setup(m => m.Add(It.IsAny<Notificacao>()))
                .Callback<Notificacao>(n => notificacaos.Add(n));


            var mockContext = new Mock<CommuniCareContext>();
            mockContext.Setup(c => c.Voluntariados).Returns(voluntariados.Object);
            mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresDbSet.Object);
            mockContext.Setup(c => c.Notificacaos).Returns(notificacaosMock.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);


            var controller = new VoluntariadosController(mockContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, "10")
                }, "mock"))
                }
            };


            var result = await controller.AceitarVoluntario(1, 10);


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Voluntário aceite com sucesso e pedido atualizado para 'Em Progresso'.", okResult.Value);

            Assert.AreEqual(EstadoVoluntariado.Aceite, voluntariado.Estado);
            Assert.AreEqual(EstadoPedido.EmProgresso, voluntariado.Pedido.Estado);

            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            notificacaosMock.Verify(m => m.Add(It.IsAny<Notificacao>()), Times.Exactly(2));
            Assert.AreEqual(2, notificacaos.Count);
        }

        /// <summary>
        /// Testa se o método AceitarVoluntario retorna Ok quando um voluntário é aceito com sucesso,
        /// mas o número de voluntários ainda não atinge o necessário para alterar o estado do pedido de ajuda.
        /// Verifica também a criação de notificações e a persistência das alterações.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task AceitarVoluntario_DeveRetornarOk_SeVoluntarioAceitoSemAtualizarPedido()
        {

            var pedido = new PedidoAjuda
            {
                PedidoId = 1,
                UtilizadorId = 5,
                NPessoas = 2,
                Estado = EstadoPedido.Pendente,
                Voluntariados = new List<Voluntariado>()
            };

            var voluntariado = new Voluntariado
            {
                IdVoluntariado = 1,
                Estado = EstadoVoluntariado.Pendente,
                PedidoId = 1,
                UtilizadorId = 10,
                Pedido = pedido,
                Utilizador = new Utilizador { UtilizadorId = 10 }
            };
            pedido.Voluntariados.Add(voluntariado);


            var voluntariados = new List<Voluntariado> { voluntariado }.AsQueryable().BuildMockDbSet();


            var adminUser = new Utilizador { UtilizadorId = 10, TipoUtilizadorId = 2 };
            var mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();
            mockUtilizadoresDbSet.Setup(m => m.FindAsync(10)).ReturnsAsync(adminUser);


            var notificacaos = new List<Notificacao>();
            var mockNotificacaosDbSet = new Mock<DbSet<Notificacao>>();
            mockNotificacaosDbSet.Setup(m => m.Add(It.IsAny<Notificacao>()))
                .Callback<Notificacao>(n => notificacaos.Add(n));


            var mockContext = new Mock<CommuniCareContext>();
            mockContext.Setup(c => c.Voluntariados).Returns(voluntariados.Object);
            mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresDbSet.Object);
            mockContext.Setup(c => c.Notificacaos).Returns(mockNotificacaosDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);


            var controller = new VoluntariadosController(mockContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                      new Claim(ClaimTypes.NameIdentifier, "10")
                    }, "mock"))
                }
            };


            var result = await controller.AceitarVoluntario(1, 10); ;


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Voluntário aceite com sucesso.", okResult.Value);
            Assert.AreEqual(EstadoVoluntariado.Aceite, voluntariado.Estado);
            Assert.AreEqual(EstadoPedido.Pendente, voluntariado.Pedido.Estado);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            mockNotificacaosDbSet.Verify(m => m.Add(It.IsAny<Notificacao>()), Times.Exactly(2));
        }

        //// <summary>
        /// Testa se o método AceitarVoluntario retorna Unauthorized quando o utilizador não está autenticado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task AceitarVoluntario_DeveRetornarUnauthorized_SeUsuarioNaoAutenticado()
        {

            var mockContext = new Mock<CommuniCareContext>();
            var controller = new VoluntariadosController(mockContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };


            var result = await controller.AceitarVoluntario(1, 10); ;


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorizedResult.Value);
        }

        /// <summary>
        /// Testa se o método AceitarVoluntario retorna BadRequest quando o voluntariado não está pendente.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task AceitarVoluntario_DeveRetornarBadRequest_SeVoluntariadoNaoPendente()
        {
           
            var pedido = new PedidoAjuda
            {
                PedidoId = 1,
                UtilizadorId = 5,
                NPessoas = 2,
                Estado = EstadoPedido.Pendente,
                Voluntariados = new List<Voluntariado>()
            };

            var voluntariado = new Voluntariado
            {
                IdVoluntariado = 1,
                Estado = EstadoVoluntariado.Aceite,  
                PedidoId = 1,
                UtilizadorId = 10,
                Pedido = pedido,
                Utilizador = new Utilizador { UtilizadorId = 10 }
            };
            pedido.Voluntariados.Add(voluntariado);

            var mockVoluntariados = new List<Voluntariado> { voluntariado }
                                    .AsQueryable()
                                    .BuildMockDbSet();

            
            var admin = new Utilizador { UtilizadorId = 10, TipoUtilizadorId = 2 };
            var mockUtilizadores = new List<Utilizador> { admin }
                                   .AsQueryable()
                                   .BuildMockDbSet();

            var ctx = new Mock<CommuniCareContext>();
            ctx.Setup(c => c.Voluntariados).Returns(mockVoluntariados.Object);
            ctx.Setup(c => c.Utilizadores).Returns(mockUtilizadores.Object);

            
            ctx.Setup(c => c.Utilizadores.FindAsync(It.IsAny<object[]>()))
               .ReturnsAsync((object[] ids) =>
               {
                   var id = (int)ids[0];
                   return mockUtilizadores.Object.FirstOrDefault(u => u.UtilizadorId == id);
               });

            
            ctx.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var controller = new VoluntariadosController(ctx.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[] { new Claim(ClaimTypes.NameIdentifier, "10") },
                            "TestAuth"))
                }
            };

            
            var result = await controller.AceitarVoluntario(
                pedido.PedidoId,
                voluntariado.UtilizadorId!.Value);

           
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badRequest = (BadRequestObjectResult)result;
            StringAssert.Contains(badRequest.Value?.ToString(), "não está pendente");
        }

        #endregion

    }

}