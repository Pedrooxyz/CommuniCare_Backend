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

namespace CommuniCareTests
{
    [TestClass]
    public class PedidoAjudaTests
    {
        private PedidosAjudaController _controller;
        private Mock<DbSet<PedidoAjuda>> _mockPedidosAjudaDbSet;
        private Mock<DbSet<Utilizador>> _mockUtilizadoresDbSet;
        private Mock<CommuniCareContext> _mockContext;

        [TestInitialize]
        public void Setup()
        {

            

            // Mock de DbSet para PedidosAjuda e Utilizadores
            _mockPedidosAjudaDbSet = new Mock<DbSet<PedidoAjuda>>();
            _mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();
            _mockContext = new Mock<CommuniCareContext>();

            // Configurando o mock para retornar os DbSets
            _mockContext.Setup(m => m.PedidosAjuda).Returns(_mockPedidosAjudaDbSet.Object);
            _mockContext.Setup(m => m.Utilizadores).Returns(_mockUtilizadoresDbSet.Object);

            // Inicializando o controller com o mock do contexto
            _controller = new PedidosAjudaController(_mockContext.Object);
        }

        #region CriarPedidoAjuda

        [TestMethod]
        public async Task CriarPedidoAjuda_ValidData_ReturnsOk()
        {
            //  Arrange 
            var dto = new PedidoAjudaDTO
            {
                DescPedido = "Preciso de ajuda com o jardim.",
                NHoras = 2,
                NPessoas = 1,
                HorarioAjuda = DateTime.Now,
                FotografiaPA = "image.jpg"
            };

            
            var requester = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 1, NomeUtilizador = "João" };
            var admin = new Utilizador { UtilizadorId = 7, TipoUtilizadorId = 2 };

            var utilizadoresDb = new[] { requester, admin }
                                 .AsQueryable()
                                 .BuildMockDbSet();

            
            utilizadoresDb
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(requester);

            _mockContext.Setup(c => c.Utilizadores).Returns(utilizadoresDb.Object);

            
            var pedidosDb = new List<PedidoAjuda>().AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidosDb.Object);

            
            var notificacoesDb = new Mock<DbSet<Notificacao>>();
            _mockContext.Setup(c => c.Notificacaos).Returns(notificacoesDb.Object);

            
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                             new ClaimsIdentity(
                               new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth"))
                }
            };

            //  Act 
            var result = await _controller.CriarPedidoAjuda(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;

            
            var msg = okResult.Value
                              .GetType()
                              .GetProperty("Mensagem")?
                              .GetValue(okResult.Value, null) as string;

            Assert.AreEqual(
                "Pedido de ajuda criado com sucesso. Notificações enviadas aos administradores.",
                msg);

        }


        [TestMethod]
        public async Task CriarPedidoAjuda_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var pedidoData = new PedidoAjudaDTO
            {
                DescPedido = "",
                NHoras = 0,
                NPessoas = 0,
                HorarioAjuda = DateTime.Now.AddDays(-1),
                FotografiaPA = ""
            };

            // Criando os claims para o utilizador autenticado
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };

            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.CriarPedidoAjuda(pedidoData);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var actionResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Dados inválidos.", actionResult.Value);
        }

        #endregion

        #region RejeitarPedido

        [TestMethod]
        public async Task RejeitarPedidoAjuda_ValidAdmin_ReturnsOk()
        {
            //  Arrange 
            var pedidoId = 1;

            
            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.Pendente,
                UtilizadorId = 3           
            };

            
            var pedidos = new List<PedidoAjuda> { pedido }
                          .AsQueryable()
                          .BuildMockDbSet();
            _mockContext.SetupGet(c => c.PedidosAjuda).Returns(pedidos.Object);

            
            var admin = new Utilizador { UtilizadorId = 2, TipoUtilizadorId = 2 };

            
            var utilizadores = new List<Utilizador> { admin }
                               .AsQueryable()
                               .BuildMockDbSet();
            _mockContext.SetupGet(c => c.Utilizadores).Returns(utilizadores.Object);

            
            utilizadores
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(admin);

            
            var notificacoes = new Mock<DbSet<Notificacao>>();
            _mockContext.SetupGet(c => c.Notificacaos).Returns(notificacoes.Object);

            
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "2") };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };

            
            var result = await _controller.RejeitarPedidoAjuda(pedidoId);

            
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual("Pedido de ajuda rejeitado com sucesso.", ok.Value);

            
            Assert.AreEqual(EstadoPedido.Rejeitado, pedido.Estado);
        }



        [TestMethod]
        public async Task RejeitarPedidoAjuda_Unauthorized_ReturnsUnauthorized()
        {
            // Arrange
            var pedidoId = 1;
            var user = new Utilizador
            {
                UtilizadorId = 1,
                TipoUtilizadorId = 1, // Não administrador
                NomeUtilizador = "João"
            };

            var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UtilizadorId.ToString())
    };

            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.RejeitarPedidoAjuda(pedidoId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        [TestMethod]
        public async Task RejeitarPedidoAjuda_PedidoNaoEncontrado_ReturnsNotFound()
        {
            //  Arrange 
            int pedidoIdInexistente = 999;

            var pedidos = new List<PedidoAjuda>()
                          .AsQueryable()
                          .BuildMockDbSet();
            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidos.Object);

            var admin = new Utilizador { UtilizadorId = 2, TipoUtilizadorId = 2 };
            var utilizadores = new List<Utilizador> { admin }
                               .AsQueryable()
                               .BuildMockDbSet();

            
            _mockContext.Setup(c => c.Utilizadores).Returns(utilizadores.Object);
            
            utilizadores.Setup(d => d.FindAsync(It.IsAny<object[]>())).ReturnsAsync(admin);

            var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "2")
    };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(userClaims))
                }
            };

            // Act 
            var result = await _controller.RejeitarPedidoAjuda(pedidoIdInexistente);

            //  Assert 
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFound = (NotFoundObjectResult)result;
            Assert.AreEqual("Pedido de ajuda não encontrado.", notFound.Value);
        }


        #endregion

        #region ValidarPedido

        [TestMethod]
        public async Task ValidarPedidoAjuda_InvalidUser_ReturnsUnauthorized()
        {
            // Arrange: Sem claims, simulando utilizador não autenticado
            var userIdentity = new ClaimsIdentity(); // Vazio
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.ValidarPedidoAjuda(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var actionResult = (UnauthorizedObjectResult)result;
            Assert.AreEqual("Utilizador não autenticado.", actionResult.Value);
        }


        [TestMethod]
        public async Task ValidarPedidoAjuda_NotAdminUser_ReturnsForbid()
        {
            // Arrange: Criar dados para o pedido
            var pedidoId = 1;
            var user = new Utilizador { UtilizadorId = 2, TipoUtilizadorId = 1 }; // Não é administrador

            // Mock do comportamento do DbContext
            _mockUtilizadoresDbSet.Setup(m => m.FindAsync(2)).ReturnsAsync(user);

            // Criando os claims para o utilizador não administrador
            var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "2")
        };
            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act: Chamar o método ValidarPedidoAjuda
            var result = await _controller.ValidarPedidoAjuda(pedidoId);

            // Assert: Verificar que o resultado é Forbid
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        [TestMethod]
        public async Task ValidarPedidoAjuda_PedidoNaoEncontrado_ReturnsNotFound()
        {
            // Arrange: Criar dados para o pedido
            var pedidoId = 999; // ID de pedido inexistente
            var user = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 2 }; // Administrador

            // Mock do comportamento do DbContext
            _mockUtilizadoresDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(user);
            _mockPedidosAjudaDbSet.Setup(m => m.FindAsync(pedidoId)).ReturnsAsync((PedidoAjuda)null); // Pedido não encontrado

            // Criando os claims para o utilizador administrador
            var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act: Chamar o método ValidarPedidoAjuda
            var result = await _controller.ValidarPedidoAjuda(pedidoId);

            // Assert: Verificar que o resultado é NotFound
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var actionResult = (NotFoundObjectResult)result;
            Assert.AreEqual("Pedido de ajuda não encontrado.", actionResult.Value);
        }

        [TestMethod]
        public async Task ValidarPedidoAjuda_PedidoJaValidado_ReturnsBadRequest()
        {
            // Arrange: Criar dados para o pedido
            var pedidoId = 1;
            var user = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 2 }; // Administrador
            var pedido = new PedidoAjuda { PedidoId = pedidoId, Estado = EstadoPedido.Aberto, UtilizadorId = 1 }; // Pedido já validado

            // Mock do comportamento do DbContext
            _mockUtilizadoresDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(user);
            _mockPedidosAjudaDbSet.Setup(m => m.FindAsync(pedidoId)).ReturnsAsync(pedido);

            // Criando os claims para o utilizador administrador
            var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act: Chamar o método ValidarPedidoAjuda
            var result = await _controller.ValidarPedidoAjuda(pedidoId);

            // Assert: Verificar que o resultado é BadRequest
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var actionResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Este pedido já foi validado ou está em progresso/concluído.", actionResult.Value);
        }

        [TestMethod]
        public async Task ValidarPedidoAjuda_Success_ReturnsOk()
        {
            //  Arrange 
            var pedidoId = 1;

            var admin = new Utilizador
            {
                UtilizadorId = 1,
                NomeUtilizador = "Admin",
                TipoUtilizadorId = 2           // 2 == administrador
            };

            var outro = new Utilizador
            {
                UtilizadorId = 2,
                NomeUtilizador = "Outro",
                TipoUtilizadorId = 1
            };

            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.Pendente,
                UtilizadorId = admin.UtilizadorId
            };

            
            var utilizadores = new[] { admin, outro }.AsQueryable();
            var utilizadoresDbSet = utilizadores.BuildMockDbSet();

            utilizadoresDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                             .ReturnsAsync((object[] keys) =>
                             {
                                 var id = (int)keys[0];
                                 return utilizadores.FirstOrDefault(u => u.UtilizadorId == id);
                             });

            _mockContext.Setup(c => c.Utilizadores).Returns(utilizadoresDbSet.Object);

            
            var pedidoList = new[] { pedido }.AsQueryable();
            var pedidosDbSet = pedidoList.BuildMockDbSet();

            pedidosDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                        .ReturnsAsync((object[] keys) =>
                        {
                            var id = (int)keys[0];
                            return pedidoList.FirstOrDefault(p => p.PedidoId == id);
                        });

            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidosDbSet.Object);

            
            var notificacoesDbSet = new List<Notificacao>().AsQueryable()
                                                           .BuildMockDbSet();
            _mockContext.Setup(c => c.Notificacaos).Returns(notificacoesDbSet.Object);

            
            var user = new ClaimsPrincipal(
                           new ClaimsIdentity(
                               new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.ValidarPedidoAjuda(pedidoId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(
                "Pedido de ajuda validado com sucesso e colocado como 'Aberto'.",
                ok.Value);
        }


        #endregion

        #region Voluntariar

        [TestMethod]
        public async Task Voluntariar_Success_ReturnsOk()
        {
            // Arrange
            int pedidoId = 1;
            int utilizadorId = 3;

            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.Aberto,
                NPessoas = 3,
                Voluntariados = new List<Voluntariado>()
            };

            var user = new Utilizador { UtilizadorId = utilizadorId, NomeUtilizador = "TesteUser", TipoUtilizadorId = 1 };
            var admin = new Utilizador { UtilizadorId = 2, TipoUtilizadorId = 2 };

            var pedidos = new List<PedidoAjuda> { pedido }.AsQueryable().BuildMockDbSet();
            var voluntariados = new List<Voluntariado>().AsQueryable().BuildMockDbSet();
            var utilizadores = new List<Utilizador> { user, admin }.AsQueryable().BuildMockDbSet();
            var notificacoes = new List<Notificacao>().AsQueryable().BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidos.Object);
            _mockContext.Setup(c => c.Voluntariados).Returns(voluntariados.Object);
            _mockContext.Setup(c => c.Utilizadores).Returns(utilizadores.Object);
            _mockContext.Setup(c => c.Notificacaos).Returns(notificacoes.Object);
            _mockContext.Setup(c => c.Utilizadores.FindAsync(utilizadorId)).ReturnsAsync(user);

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString())
    }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            // Act
            var result = await _controller.Voluntariar(pedidoId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = result as OkObjectResult;
            Assert.AreEqual("Pedido de voluntariado registado com sucesso. Aguardando aprovação do administrador.", ok.Value);
        }

        [TestMethod]
        public async Task Voluntariar_PedidoEmProgresso_ReturnsBadRequest()
        {
            var pedido = new PedidoAjuda
            {
                PedidoId = 1,
                Estado = EstadoPedido.EmProgresso
            };

            var pedidos = new List<PedidoAjuda> { pedido }.AsQueryable().BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidos.Object);

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "3")
    }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            var result = await _controller.Voluntariar(1);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Pedido não encontrado ou já fechado.", ((BadRequestObjectResult)result).Value);
        }

        [TestMethod]
        public async Task Voluntariar_NumeroMaximoAtingido_ReturnsBadRequest()
        {
            var pedido = new PedidoAjuda
            {
                PedidoId = 1,
                Estado = EstadoPedido.Aberto,
                NPessoas = 1,
                Voluntariados = new List<Voluntariado> { new Voluntariado { UtilizadorId = 2 } }
            };

            var pedidos = new List<PedidoAjuda> { pedido }.AsQueryable().BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidos.Object);

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "3")
    }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            var result = await _controller.Voluntariar(1);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Número máximo de voluntários já atingido para este pedido.", ((BadRequestObjectResult)result).Value);
        }


        [TestMethod]
        public async Task Voluntariar_JaVoluntariado_ReturnsBadRequest()
        {
            var pedido = new PedidoAjuda
            {
                PedidoId = 1,
                Estado = EstadoPedido.Aberto,
                Voluntariados = new List<Voluntariado> { new Voluntariado { UtilizadorId = 3 } }
            };

            var pedidos = new List<PedidoAjuda> { pedido }.AsQueryable().BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidos.Object);

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "3")
    }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            var result = await _controller.Voluntariar(1);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Utilizador já se voluntariou para este pedido.", ((BadRequestObjectResult)result).Value);
        }



        #endregion

        #region ConcluirPedido

        [TestMethod]
        public async Task ConcluirPedidoAjuda_ValidUser_ReturnsOk()
        {
            //  Arrange 
            const int pedidoId = 1;
            const int requesterId = 2;          

            
            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.EmProgresso,
                UtilizadorId = requesterId,
                Utilizador = new Utilizador { UtilizadorId = requesterId, NomeUtilizador = "User" },
                Voluntariados = new List<Voluntariado>()
            };

            
            var pedidosDbSet = new[] { pedido }
                               .AsQueryable()
                               .BuildMockDbSet();
            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidosDbSet.Object);

           
            var adminsDbSet = new[]
            {
        new Utilizador { UtilizadorId = 3, TipoUtilizadorId = 2 }
    }
            .AsQueryable()
            .BuildMockDbSet();
            _mockContext.Setup(c => c.Utilizadores).Returns(adminsDbSet.Object);

            
            var notificacoesDbSet = new Mock<DbSet<Notificacao>>();
            _mockContext.Setup(c => c.Notificacaos).Returns(notificacoesDbSet.Object);

            
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[] { new Claim(ClaimTypes.NameIdentifier, requesterId.ToString()) },
                            "TestAuth"))
                }
            };

            //  Act 
            var result = await _controller.ConcluirPedidoAjuda(pedidoId);

            //  Assert 
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(
                "O pedido foi marcado como concluído. Os administradores foram notificados para validar a conclusão.",
                ok.Value);

            
            Assert.AreEqual(EstadoPedido.Concluido, pedido.Estado);

           
            notificacoesDbSet.Verify(d => d.Add(It.IsAny<Notificacao>()), Times.Once);

            
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ConcluirPedidoAjuda_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var pedidoId = 1;
            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.EmProgresso,
                UtilizadorId = 2
            };

            // Simulação do DbSet de PedidoAjuda
            var mockDbSetPedidos = new Mock<DbSet<PedidoAjuda>>();
            mockDbSetPedidos.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(pedido);
            _mockContext.Setup(c => c.PedidosAjuda).Returns(mockDbSetPedidos.Object);

            // Simulação de utilizador não autenticado (sem Claim)
            var userClaims = new List<Claim>(); // Nenhum Claim
            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act: Chamar o método ConcluirPedidoAjuda
            var result = await _controller.ConcluirPedidoAjuda(pedidoId);

            // Assert: Verificar que o resultado é Unauthorized
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var actionResult = (UnauthorizedObjectResult)result;
            Assert.AreEqual("Utilizador não autenticado.", actionResult.Value);
        }

        [TestMethod]
        public async Task ConcluirPedidoAjuda_PedidoNotFound_ReturnsNotFound()
        {
            
            var pedidoId = 1;

            
            var pedidos = new List<PedidoAjuda>().AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidos.Object);

            
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "2")
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(userClaims))
                }
            };

            
            var result = await _controller.ConcluirPedidoAjuda(pedidoId);

            
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFound = (NotFoundObjectResult)result;
            Assert.AreEqual("Pedido de ajuda não encontrado.", notFound.Value);
        }



        [TestMethod]
        public async Task ConcluirPedidoAjuda_UserIsNotRequester_ReturnsForbid()
        {
            
            var pedidoId = 1;

            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.EmProgresso,
                UtilizadorId = 3 // Utilizador diferente do que tenta concluir o pedido
            };

            
            var pedidosDbSet = new[] { pedido }
                               .AsQueryable()
                               .BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda)
                        .Returns(pedidosDbSet.Object);

            // Mock do utilizador autenticado (ID 2, que não é o requisitante)
            var user = new ClaimsPrincipal(
                           new ClaimsIdentity(
                               new[] { new Claim(ClaimTypes.NameIdentifier, "2") }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act 
            var result = await _controller.ConcluirPedidoAjuda(pedidoId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));

        }


        
       [TestMethod]
        public async Task ConcluirPedidoAjuda_PedidoNotInProgress_ReturnsBadRequest()
        {
            //  Arrange 
            var pedidoId = 1;

            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.Concluido,   // Estado já está em "Concluído", não está "Em Progresso"
                UtilizadorId = 2                         
            };

            
            var pedidosDbSet = new[] { pedido }
                               .AsQueryable()
                               .BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda)
                        .Returns(pedidosDbSet.Object);

            // Logged-in user is the requester (id = 2)
            var user = new ClaimsPrincipal(
                           new ClaimsIdentity(
                               new[] { new Claim(ClaimTypes.NameIdentifier, "2") }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            //  Act 
            var result = await _controller.ConcluirPedidoAjuda(pedidoId);

            //  Assert 
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("O pedido não está em progresso ou já foi concluído.", bad.Value);
        }

        #endregion

        #region ValidarConclusao

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var pedidoId = 1;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // sem claims
            };

            // Act
            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var objectResult = (UnauthorizedObjectResult)result;
            Assert.AreEqual("Utilizador não autenticado.", objectResult.Value);
        }

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_PedidoNotFound_ReturnsNotFound()
        {
            //  Arrange 
            const int pedidoId = 1;
            const int utilizadorId = 2;

           
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString()) };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };

            
            var admin = new Utilizador { UtilizadorId = utilizadorId, TipoUtilizadorId = 2 };

            var utilizadores = new List<Utilizador> { admin }
                               .AsQueryable()
                               .BuildMockDbSet();

            _mockContext.Setup(c => c.Utilizadores).Returns(utilizadores.Object);
            utilizadores
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(admin);

            
            var pedidos = new List<PedidoAjuda>()     // lista vazia
                          .AsQueryable()
                          .BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidos.Object);

            //  Act
            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);

            //  Assert 
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFound = (NotFoundObjectResult)result;
            Assert.AreEqual("Pedido de ajuda não encontrado.", notFound.Value);
        }


        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_UserIsNotAdmin_ReturnsForbid()
        {
            // Arrange
            var pedidoId = 1;
            var utilizadorId = 2;

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString())
    }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var mockDbSetUtilizadores = new Mock<DbSet<Utilizador>>();
            mockDbSetUtilizadores.Setup(m => m.FindAsync(utilizadorId))
                .ReturnsAsync(new Utilizador { UtilizadorId = utilizadorId, TipoUtilizadorId = 1 }); // não admin

            _mockContext.Setup(c => c.Utilizadores).Returns(mockDbSetUtilizadores.Object);

            // Act
            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_PedidoNotConcluded_ReturnsBadRequest()
        {
            //  Arrange 
            const int pedidoId = 1;
            const int utilizadorId = 2;

            
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString()) };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };

            
            var admin = new Utilizador { UtilizadorId = utilizadorId, TipoUtilizadorId = 2 };

            var utilizadores = new List<Utilizador> { admin }
                               .AsQueryable()
                               .BuildMockDbSet();

            _mockContext.Setup(c => c.Utilizadores).Returns(utilizadores.Object);
            utilizadores
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(admin);

           
            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.Pendente,
                Utilizador = new Utilizador(),
                Voluntariados = new List<Voluntariado>()       
            };

            
            var pedidos = new List<PedidoAjuda> { pedido }
                          .AsQueryable()
                          .BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidos.Object);

            //  Act 
            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);

            //  Assert 
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("O pedido não ainda não foi concluído.", bad.Value);
        }

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_RecetorIsNull_ReturnsBadRequest()
        {
            //  Arrange 
            const int pedidoId = 1;
            const int utilizadorId = 2;

            
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString()) };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };

            
            var admin = new Utilizador { UtilizadorId = utilizadorId, TipoUtilizadorId = 2 };

            var utilizadores = new List<Utilizador> { admin }
                               .AsQueryable()
                               .BuildMockDbSet();

            _mockContext.Setup(c => c.Utilizadores).Returns(utilizadores.Object);
            utilizadores
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(admin);

            
            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.Concluido,
                Utilizador = null,                 
                Voluntariados = new List<Voluntariado>()
            };

            var pedidos = new List<PedidoAjuda> { pedido }
                          .AsQueryable()
                          .BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidos.Object);

            //  Act 
            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);

            //  Assert 
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("Não foi possível determinar o recetor do pedido.", bad.Value);
        }

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_ValidRequest_ReturnsOk()
        {
            //  Arrange 
            const int pedidoId = 1;
            const int adminId = 2;

            
            var admin = new Utilizador { UtilizadorId = adminId, TipoUtilizadorId = 2 };

            
            var recetor = new Utilizador { UtilizadorId = 3, NumCares = 10 };

            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.Concluido,
                Utilizador = recetor,
                UtilizadorId = recetor.UtilizadorId,
                RecompensaCares = 5,
                Voluntariados = new List<Voluntariado>
        {
            new Voluntariado { UtilizadorId = 99 }
        }
            };

            
            var utilizadores = new[] { admin, recetor }
                               .AsQueryable()
                               .BuildMockDbSet();

            utilizadores
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] keys) =>
                {
                    int id = (int)keys[0];
                    return utilizadores.Object.First(u => u.UtilizadorId == id);
                });

            _mockContext.Setup(c => c.Utilizadores).Returns(utilizadores.Object);

            
            var pedidosDbSet = new[] { pedido }
                               .AsQueryable()
                               .BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidosDbSet.Object);

            
            var transacoesDb = new Mock<DbSet<Transacao>>();
            var transAjudaDb = new Mock<DbSet<TransacaoAjuda>>();
            var notificacoesDb = new Mock<DbSet<Notificacao>>();

            _mockContext.Setup(c => c.Transacoes).Returns(transacoesDb.Object);
            _mockContext.Setup(c => c.TransacaoAjuda).Returns(transAjudaDb.Object);
            _mockContext.Setup(c => c.Notificacaos).Returns(notificacoesDb.Object);

            
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

            //  Act 
            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);

            //  Assert 
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(
                "Pedido de ajuda concluído com sucesso. Recompensa atribuída, transação registada e notificação enviada.",
                ok.Value);

            
            Assert.AreEqual(15, recetor.NumCares);

            
            transacoesDb.Verify(d => d.Add(It.IsAny<Transacao>()), Times.Once);
            transAjudaDb.Verify(d => d.Add(It.IsAny<TransacaoAjuda>()), Times.Once);
            notificacoesDb.Verify(d => d.Add(It.IsAny<Notificacao>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
                                   Times.Once);
        }



        #endregion

        #region GetPedidosAjudaDisponiveis

        [TestMethod]
        public async Task GetPedidosAjudaDisponiveis_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange: Sem claims, simulando utilizador não autenticado
            var userIdentity = new ClaimsIdentity(); // Vazio
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.GetPedidosAjudaDisponiveis();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));  // Verifique se a resposta é Unauthorized
            var actionResult = result.Result as UnauthorizedObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual("Utilizador não autenticado.", actionResult.Value);
        }

        [TestMethod]
        public async Task GetPedidosAjudaDisponiveis_Success_ReturnsPedidosDisponiveis()
        {
            // Arrange: Criar dados simulados
            var pedidos = new List<PedidoAjuda>
    {
        new PedidoAjuda { PedidoId = 1, UtilizadorId = 2, Estado = EstadoPedido.Aberto },
        new PedidoAjuda { PedidoId = 2, UtilizadorId = 3, Estado = EstadoPedido.Aberto }
    };

            var userId = 1;
            var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
    };
            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            var mockDbSetPedidos = new Mock<DbSet<PedidoAjuda>>();
            mockDbSetPedidos.Setup(p => p.Where(It.IsAny<Expression<Func<PedidoAjuda, bool>>>())).Returns(pedidos.AsQueryable());
            mockDbSetPedidos.Setup(p => p.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(pedidos);
            _mockContext.Setup(c => c.PedidosAjuda).Returns(mockDbSetPedidos.Object);

            // Act
            var result = await _controller.GetPedidosAjudaDisponiveis();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));  // Verifique se a resposta é Ok
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedPedidos = okResult.Value as IEnumerable<PedidoAjuda>;
            Assert.IsNotNull(returnedPedidos);
            Assert.AreEqual(2, returnedPedidos.Count());
        }

        #endregion

        #region GetMeusPedidosAjuda

        [TestMethod]
        public async Task GetMeusPedidosAjuda_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange: Sem claims, simulando utilizador não autenticado
            var userIdentity = new ClaimsIdentity(); // Vazio
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.GetMeusPedidosAjuda();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));  // Verifique se a resposta é Unauthorized
            var actionResult = result.Result as UnauthorizedObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual("Utilizador não autenticado.", actionResult.Value);
        }

        [TestMethod]
        public async Task GetMeusPedidosAjuda_Success_ReturnsMeusPedidos()
        {
            //  Arrange 
            const int userId = 1;

            var pedidos = new List<PedidoAjuda>
    {
        new PedidoAjuda { PedidoId = 1, UtilizadorId = userId, Estado = EstadoPedido.Pendente },
        new PedidoAjuda { PedidoId = 2, UtilizadorId = userId, Estado = EstadoPedido.Pendente },
        new PedidoAjuda { PedidoId = 3, UtilizadorId = 99,   Estado = EstadoPedido.Pendente } // deve ser filtrado
    };

            
            var pedidosDbSet = pedidos.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidosDbSet.Object);

            // utilizador autenticado
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

            //  Act 
            var result = await _controller.GetMeusPedidosAjuda();

            //  Assert 
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result.Result;

            var returned = ok.Value as IEnumerable<PedidoAjuda>;
            Assert.IsNotNull(returned);

            
            var list = returned.ToList();
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.All(p => p.UtilizadorId == userId));
        }

        #endregion






    }
}
