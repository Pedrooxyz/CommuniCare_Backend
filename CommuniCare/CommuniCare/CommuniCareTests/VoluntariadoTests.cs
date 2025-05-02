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

    [TestClass]
    public class VoluntariadoControllerTests
    {
        private Mock<CommuniCareContext> _mockContext;
        private Mock<DbSet<Artigo>> _mockArtigos;
        private Mock<DbSet<Favoritos>> _mockFavoritos;
        private FavoritosController _controller;

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
            voluntariados.Setup(m => m.Remove(It.IsAny<Voluntariado>())).Callback<Voluntariado>(v => { }); // simulate removal

            var admin = new Utilizador { UtilizadorId = 10, TipoUtilizadorId = 2 };
            var mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();
            mockUtilizadoresDbSet.Setup(m => m.FindAsync(10)).ReturnsAsync(admin); // correct way to mock FindAsync

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
            var result = await controller.RejeitarVoluntario(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Voluntário rejeitado com sucesso.", okResult.Value);

            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            notificacaosMock.Verify(m => m.Add(It.IsAny<Notificacao>()), Times.Once());
        }



        [TestMethod]
        public async Task RejeitarVoluntario_DeveRetornarUnauthorized_SeUsuarioNaoAutenticado()
        {
            // Arrange
            var mockContext = new Mock<CommuniCareContext>();
            var controller = new VoluntariadosController(mockContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = await controller.RejeitarVoluntario(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorizedResult.Value);
        }

        [TestMethod]
        public async Task RejeitarVoluntario_DeveRetornarForbid_SeUsuarioNaoForAdmin()
        {
            // Arrange
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

            // Act
            var result = await controller.RejeitarVoluntario(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
            var forbidResult = result as ForbidResult;
            Assert.IsNotNull(forbidResult);
        }

        [TestMethod]
        public async Task RejeitarVoluntario_DeveRetornarForbid_SeAdminNaoExiste()
        {
            // Arrange
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

            // Act
            var result = await controller.RejeitarVoluntario(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
            var forbidResult = result as ForbidResult;
            Assert.IsNotNull(forbidResult);
        }

        [TestMethod]
        public async Task RejeitarVoluntario_DeveRetornarNotFound_SeVoluntariadoNaoExiste()
        {
            // Arrange
            var data = new List<Voluntariado>().AsQueryable(); // Nenhum voluntariado
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

            // Act
            var result = await controller.RejeitarVoluntario(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Voluntariado não encontrado ou já foi processado.", notFoundResult.Value);
        }

        #endregion

        #region AceitarVoluntario

        
            [TestMethod]
            public async Task AceitarVoluntario_DeveRetornarOk_SeVoluntarioAceitoEPedidoEmProgresso()
            {
                // Arrange
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

                // Mock Voluntariados DbSet
                var voluntariados = new List<Voluntariado> { voluntariado }.AsQueryable().BuildMockDbSet();

                // Mock Utilizadores DbSet with FindAsync
                var adminUser = new Utilizador { UtilizadorId = 10, TipoUtilizadorId = 2 };
                var mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();
                mockUtilizadoresDbSet.Setup(m => m.FindAsync(10)).ReturnsAsync(adminUser);

                // Mock Notificacoes
                var notificacaos = new List<Notificacao>();
                var notificacaosMock = new Mock<DbSet<Notificacao>>();
                notificacaosMock.Setup(m => m.Add(It.IsAny<Notificacao>()))
                    .Callback<Notificacao>(n => notificacaos.Add(n));

                // Mock context
                var mockContext = new Mock<CommuniCareContext>();
                mockContext.Setup(c => c.Voluntariados).Returns(voluntariados.Object);
                mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresDbSet.Object);
                mockContext.Setup(c => c.Notificacaos).Returns(notificacaosMock.Object);
                mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

                // Controller setup with authenticated admin user
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
                var result = await controller.AceitarVoluntario(1);

                // Assert
                Assert.IsInstanceOfType(result, typeof(OkObjectResult));
                var okResult = result as OkObjectResult;
                Assert.AreEqual("Voluntário aceite com sucesso e pedido atualizado para 'Em Progresso'.", okResult.Value);

                Assert.AreEqual(EstadoVoluntariado.Aceite, voluntariado.Estado);
                Assert.AreEqual(EstadoPedido.EmProgresso, voluntariado.Pedido.Estado);

                mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
                notificacaosMock.Verify(m => m.Add(It.IsAny<Notificacao>()), Times.Exactly(2));
                Assert.AreEqual(2, notificacaos.Count);
            }

            [TestMethod]
        public async Task AceitarVoluntario_DeveRetornarOk_SeVoluntarioAceitoSemAtualizarPedido()
        {
            // Arrange
            var pedido = new PedidoAjuda
            {
                PedidoId = 1,
                UtilizadorId = 5,
                NPessoas = 2,
                Estado = EstadoPedido.Pendente,
                Voluntariados = new List<Voluntariado>() // Nenhum voluntário aceito
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

            // Mock Voluntariados DbSet
            var voluntariados = new List<Voluntariado> { voluntariado }.AsQueryable().BuildMockDbSet();

            // Properly mock FindAsync for admin user
            var adminUser = new Utilizador { UtilizadorId = 10, TipoUtilizadorId = 2 };
            var mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();
            mockUtilizadoresDbSet.Setup(m => m.FindAsync(10)).ReturnsAsync(adminUser);

            // Mock Notificacoes
            var notificacaos = new List<Notificacao>();
            var mockNotificacaosDbSet = new Mock<DbSet<Notificacao>>();
            mockNotificacaosDbSet.Setup(m => m.Add(It.IsAny<Notificacao>()))
                .Callback<Notificacao>(n => notificacaos.Add(n));

            // Setup mocked context
            var mockContext = new Mock<CommuniCareContext>();
            mockContext.Setup(c => c.Voluntariados).Returns(voluntariados.Object);
            mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresDbSet.Object);
            mockContext.Setup(c => c.Notificacaos).Returns(mockNotificacaosDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Setup controller with authenticated admin user
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
            var result = await controller.AceitarVoluntario(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Voluntário aceite com sucesso.", okResult.Value);
            Assert.AreEqual(EstadoVoluntariado.Aceite, voluntariado.Estado);
            Assert.AreEqual(EstadoPedido.Pendente, voluntariado.Pedido.Estado);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            mockNotificacaosDbSet.Verify(m => m.Add(It.IsAny<Notificacao>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task AceitarVoluntario_DeveRetornarUnauthorized_SeUsuarioNaoAutenticado()
        {
            // Arrange
            var mockContext = new Mock<CommuniCareContext>();
            var controller = new VoluntariadosController(mockContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // Sem claims
            };

            // Act
            var result = await controller.AceitarVoluntario(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorizedResult.Value);
        }

        [TestMethod]
        public async Task AceitarVoluntario_DeveRetornarBadRequest_SeVoluntariadoNaoPendente()
        {
            // Arrange
            var voluntariado = new Voluntariado
            {
                IdVoluntariado = 1,
                Estado = EstadoVoluntariado.Aceite, // Não pendente
                PedidoId = 1,
                UtilizadorId = 10,
                Pedido = new PedidoAjuda
                {
                    PedidoId = 1,
                    UtilizadorId = 5,
                    NPessoas = 2,
                    Estado = EstadoPedido.Pendente,
                    Voluntariados = new List<Voluntariado>()
                },
                Utilizador = new Utilizador { UtilizadorId = 10 }
            };

            var data = new List<Voluntariado> { voluntariado }.AsQueryable();
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
            mockVoluntariadoDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(voluntariado);

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

            // Act
            var result = await controller.AceitarVoluntario(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("Voluntariado não encontrado ou não está pendente.", badRequestResult.Value);
        }

        #endregion

    }

}