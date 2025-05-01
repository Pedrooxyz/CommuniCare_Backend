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
            // Arrange
            var pedidoData = new PedidoAjudaDTO
            {
                DescPedido = "Preciso de ajuda com o jardim.",
                NHoras = 2,
                NPessoas = 1,
                HorarioAjuda = DateTime.Now,
                FotografiaPA = "image.jpg"
            };

            // Criando um utilizador para simular a requisição
            var user = new Utilizador
            {
                UtilizadorId = 1,
                TipoUtilizadorId = 1,
                NomeUtilizador = "João"
            };

            // Criando os claims para o utilizador autenticado
            var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1") // O ID aqui precisa ser o mesmo que o do 'user' mockado
    };

            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Criando o DbSet mockado com um IQueryable
            var queryableUtilizadores = new List<Utilizador> { user }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Utilizador>>();

            // Configurando o mock para simular a busca do utilizador
            mockDbSet.As<IQueryable<Utilizador>>()
                     .Setup(m => m.Provider)
                     .Returns(queryableUtilizadores.Provider);
            mockDbSet.As<IQueryable<Utilizador>>()
                     .Setup(m => m.Expression)
                     .Returns(queryableUtilizadores.Expression);
            mockDbSet.As<IQueryable<Utilizador>>()
                     .Setup(m => m.ElementType)
                     .Returns(queryableUtilizadores.ElementType);
            mockDbSet.As<IQueryable<Utilizador>>()
                     .Setup(m => m.GetEnumerator())
                     .Returns(queryableUtilizadores.GetEnumerator());

            // Mockando o DbContext para retornar o DbSet mockado
            _mockContext.Setup(m => m.Utilizadores).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.CriarPedidoAjuda(pedidoData);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult)); // Espera-se que o resultado seja Ok
            var actionResult = (OkObjectResult)result;
            dynamic response = actionResult.Value;
            Assert.AreEqual("Pedido de ajuda criado com sucesso. Notificações enviadas aos administradores.", response.Mensagem);
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
            // Arrange
            var pedidoId = 1;
            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.Pendente,
                UtilizadorId = 2
            };

            // Simulação do DbSet
            var mockDbSet = new Mock<DbSet<PedidoAjuda>>();
            mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(pedido);  // Assegura que o pedido é encontrado
            _mockContext.Setup(c => c.PedidosAjuda).Returns(mockDbSet.Object);

            // Mock do SaveChangesAsync
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            // Simulação do utilizador autenticado
            var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "2")  // Administrador
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
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var actionResult = (OkObjectResult)result;
            Assert.AreEqual("Pedido de ajuda rejeitado com sucesso.", actionResult.Value);
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
            // Criar o DbContext para o teste com InMemoryDatabase isolado
            var optionsBuilder = new DbContextOptionsBuilder<CommuniCareContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()); // Garantir que é um banco de dados único

            var context = new CommuniCareContext(optionsBuilder.Options);

            // Criar um administrador válido
            var admin = new Utilizador { UtilizadorId = 1, NomeUtilizador = "admin", TipoUtilizadorId = 2 };
            context.Utilizadores.Add(admin);
            await context.SaveChangesAsync();

            // Criar o controller com o DbContext de memória
            var controller = new PedidosAjudaController(context);

            // Configurar os claims para o utilizador autenticado
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, admin.UtilizadorId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Definir um ID de pedido que não existe
            int pedidoInexistenteId = 999;

            // Chamar o método que estamos testando
            var result = await controller.RejeitarPedidoAjuda(pedidoInexistenteId);

            // Verificar se o resultado é um NotFoundObjectResult
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
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
            // Arrange
            var pedidoId = 1;
            var admin = new Utilizador { UtilizadorId = 1, NomeUtilizador = "Admin", TipoUtilizadorId = 2 };
            var outro = new Utilizador { UtilizadorId = 2, NomeUtilizador = "Outro", TipoUtilizadorId = 1 };
            var pedido = new PedidoAjuda { PedidoId = pedidoId, Estado = EstadoPedido.Pendente, UtilizadorId = admin.UtilizadorId };

            // Mock para Utilizadores como IQueryable (para .Where/.ToListAsync)
            var utilizadores = new List<Utilizador> { admin, outro }.AsQueryable();
            var mockUtilizadoresSet = new Mock<DbSet<Utilizador>>();
            mockUtilizadoresSet.As<IQueryable<Utilizador>>().Setup(m => m.Provider).Returns(utilizadores.Provider);
            mockUtilizadoresSet.As<IQueryable<Utilizador>>().Setup(m => m.Expression).Returns(utilizadores.Expression);
            mockUtilizadoresSet.As<IQueryable<Utilizador>>().Setup(m => m.ElementType).Returns(utilizadores.ElementType);
            mockUtilizadoresSet.As<IQueryable<Utilizador>>().Setup(m => m.GetEnumerator()).Returns(utilizadores.GetEnumerator());
            mockUtilizadoresSet.Setup(m => m.FindAsync(1)).ReturnsAsync(admin);

            _mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresSet.Object);

            // Mock para PedidosAjuda
            var mockPedidosAjudaSet = new Mock<DbSet<PedidoAjuda>>();
            mockPedidosAjudaSet.Setup(m => m.FindAsync(pedidoId)).ReturnsAsync(pedido);
            _mockContext.Setup(c => c.PedidosAjuda).Returns(mockPedidosAjudaSet.Object);

            // Mock para Notificacoes (só para não falhar no Save)
            var mockNotificacoesSet = new Mock<DbSet<Notificacao>>();
            _mockContext.Setup(c => c.Notificacaos).Returns(mockNotificacoesSet.Object);

            // Mock do SaveChanges
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Mock do utilizador autenticado
            var userClaims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.ValidarPedidoAjuda(pedidoId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual("Pedido de ajuda validado com sucesso e colocado como 'Aberto'.", okResult.Value);
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
            // Arrange
            var pedidoId = 1;

            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.EmProgresso,
                UtilizadorId = 2,
                Utilizador = new Utilizador { UtilizadorId = 2, NomeUtilizador = "User" },
                Voluntariados = new List<Voluntariado>()   
            };

            
            var pedidosDbSet = new[] { pedido }
                               .AsQueryable()
                               .BuildMockDbSet();

            
            var adminsDbSet = new[]
            {
                new Utilizador { UtilizadorId = 3, TipoUtilizadorId = 2 }   // 2 == administrador
            }
            .AsQueryable()
            .BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidosDbSet.Object);
            _mockContext.Setup(c => c.Utilizadores).Returns(adminsDbSet.Object);

            
            var user = new ClaimsPrincipal(
                           new ClaimsIdentity(
                               new[] { new Claim(ClaimTypes.NameIdentifier, "2") }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            //Act 
            var result = await _controller.ConcluirPedidoAjuda(pedidoId);

            //Assert 
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(
                "O pedido foi marcado como concluído. Os administradores foram notificados para validar a conclusão.",
                ok.Value);
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
            // Arrange
            var pedidoId = 1;
            var utilizadorId = 2;

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString())
    }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            _mockContext.Setup(c => c.Utilizadores.FindAsync(utilizadorId))
                .ReturnsAsync(new Utilizador { UtilizadorId = utilizadorId, TipoUtilizadorId = 2 }); // é admin

            var mockDbSetPedidos = new Mock<DbSet<PedidoAjuda>>();
            mockDbSetPedidos.Setup(p => p.Include(It.IsAny<string>())).Returns(mockDbSetPedidos.Object);
            mockDbSetPedidos.Setup(p => p.FirstOrDefaultAsync(It.IsAny<Expression<Func<PedidoAjuda, bool>>>(), default))
                .ReturnsAsync((PedidoAjuda)null); // não encontrado

            _mockContext.Setup(c => c.PedidosAjuda).Returns(mockDbSetPedidos.Object);

            // Act
            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var objectResult = (NotFoundObjectResult)result;
            Assert.AreEqual("Pedido de ajuda não encontrado.", objectResult.Value);
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
            // Arrange
            var pedidoId = 1;
            var utilizadorId = 2;

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString())
    }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var utilizador = new Utilizador { UtilizadorId = utilizadorId, TipoUtilizadorId = 2 };
            _mockContext.Setup(c => c.Utilizadores.FindAsync(utilizadorId)).ReturnsAsync(utilizador);

            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.Pendente,
                Utilizador = new Utilizador()
            };

            var mockDbSetPedidos = new Mock<DbSet<PedidoAjuda>>();
            mockDbSetPedidos.Setup(p => p.Include(It.IsAny<string>())).Returns(mockDbSetPedidos.Object);
            mockDbSetPedidos.Setup(p => p.FirstOrDefaultAsync(It.IsAny<Expression<Func<PedidoAjuda, bool>>>(), default))
                .ReturnsAsync(pedido);

            _mockContext.Setup(c => c.PedidosAjuda).Returns(mockDbSetPedidos.Object);

            // Act
            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var objectResult = (BadRequestObjectResult)result;
            Assert.AreEqual("O pedido não ainda não foi concluído.", objectResult.Value);
        }

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_RecetorIsNull_ReturnsBadRequest()
        {
            // Arrange
            var pedidoId = 1;
            var utilizadorId = 2;

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString())
    }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var utilizador = new Utilizador { UtilizadorId = utilizadorId, TipoUtilizadorId = 2 };
            _mockContext.Setup(c => c.Utilizadores.FindAsync(utilizadorId)).ReturnsAsync(utilizador);

            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.Concluido,
                Utilizador = null
            };

            var mockDbSetPedidos = new Mock<DbSet<PedidoAjuda>>();
            mockDbSetPedidos.Setup(p => p.Include(It.IsAny<string>())).Returns(mockDbSetPedidos.Object);
            mockDbSetPedidos.Setup(p => p.FirstOrDefaultAsync(It.IsAny<Expression<Func<PedidoAjuda, bool>>>(), default))
                .ReturnsAsync(pedido);

            _mockContext.Setup(c => c.PedidosAjuda).Returns(mockDbSetPedidos.Object);

            // Act
            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var objectResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Não foi possível determinar o recetor do pedido.", objectResult.Value);
        }

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_ValidRequest_ReturnsOk()
        {
            // Arrange
            var pedidoId = 1;
            var utilizadorId = 2;
            var recetor = new Utilizador { UtilizadorId = 3, NumCares = 10 };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString())
    }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var utilizador = new Utilizador { UtilizadorId = utilizadorId, TipoUtilizadorId = 2 };
            _mockContext.Setup(c => c.Utilizadores.FindAsync(utilizadorId)).ReturnsAsync(utilizador);

            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.Concluido,
                Utilizador = recetor,
                RecompensaCares = 5,
                Voluntariados = new List<Voluntariado>
        {
            new Voluntariado { UtilizadorId = 99 }
        }
            };

            var mockDbSetPedidos = new Mock<DbSet<PedidoAjuda>>();
            mockDbSetPedidos.Setup(p => p.Include(It.IsAny<string>())).Returns(mockDbSetPedidos.Object);
            mockDbSetPedidos.Setup(p => p.FirstOrDefaultAsync(It.IsAny<Expression<Func<PedidoAjuda, bool>>>(), default))
                .ReturnsAsync(pedido);
            _mockContext.Setup(c => c.PedidosAjuda).Returns(mockDbSetPedidos.Object);

            var mockDbSetTransacoes = new Mock<DbSet<Transacao>>();
            var mockDbSetTransacaoAjuda = new Mock<DbSet<TransacaoAjuda>>();
            var mockDbSetNotificacoes = new Mock<DbSet<Notificacao>>();

            _mockContext.Setup(c => c.Transacoes).Returns(mockDbSetTransacoes.Object);
            _mockContext.Setup(c => c.TransacaoAjuda).Returns(mockDbSetTransacaoAjuda.Object);
            _mockContext.Setup(c => c.Notificacaos).Returns(mockDbSetNotificacoes.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var objectResult = (OkObjectResult)result;
            Assert.AreEqual("Pedido de ajuda concluído com sucesso. Recompensa atribuída, transação registada e notificação enviada.", objectResult.Value);
            Assert.AreEqual(15, recetor.NumCares); // recompensa aplicada
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
            // Arrange: Criar dados simulados
            var pedidos = new List<PedidoAjuda>
    {
        new PedidoAjuda { PedidoId = 1, UtilizadorId = 1, Estado = EstadoPedido.Aberto },
        new PedidoAjuda { PedidoId = 2, UtilizadorId = 1, Estado = EstadoPedido.Pendente }
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
            var result = await _controller.GetMeusPedidosAjuda();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));  // Verifique se a resposta é Ok
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedPedidos = okResult.Value as IEnumerable<PedidoAjuda>;
            Assert.IsNotNull(returnedPedidos);
            Assert.AreEqual(2, returnedPedidos.Count());
        }

        #endregion






    }
}
