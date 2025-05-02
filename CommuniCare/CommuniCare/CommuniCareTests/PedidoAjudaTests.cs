/// <summary>
/// Namespace que contém os testes unitários da aplicação CommuniCare.
/// Utiliza o framework MSTest e Moq para simular interações com a base de dados e testar o comportamento dos métodos do controlador PedidosAjudaController.
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


namespace CommuniCareTests
{
    /// <summary>
    /// Classe de testes unitários para o controlador PedidosAjudaController.
    /// Contém testes para verificar o comportamento dos métodos do controlador ao interagir com os dados de pedidos de ajuda e utilizadores.
    /// </summary>

    [TestClass]
    public class PedidoAjudaTests
    {
        private PedidosAjudaController _controller;
        private Mock<DbSet<PedidoAjuda>> _mockPedidosAjudaDbSet;
        private Mock<DbSet<Utilizador>> _mockUtilizadoresDbSet;
        private Mock<CommuniCareContext> _mockContext;

        /// <summary>
        /// Configura o ambiente para os testes, criando mocks dos DbSet e do contexto do banco de dados.
        /// Este método é executado antes de cada teste para garantir que os objetos necessários estão preparados.
        /// </summary>

        [TestInitialize]
        public void Setup()
        {

            _mockPedidosAjudaDbSet = new Mock<DbSet<PedidoAjuda>>();
            _mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();
            _mockContext = new Mock<CommuniCareContext>();


            _mockContext.Setup(m => m.PedidosAjuda).Returns(_mockPedidosAjudaDbSet.Object);
            _mockContext.Setup(m => m.Utilizadores).Returns(_mockUtilizadoresDbSet.Object);


            _controller = new PedidosAjudaController(_mockContext.Object);
        }

        #region CriarPedidoAjuda

        /// <summary>
        /// Testa o comportamento do método CriarPedidoAjuda quando os dados fornecidos são válidos.
        /// Espera-se que o método retorne um sucesso (status 200) com a mensagem de confirmação de criação do pedido e envio de notificações aos administradores.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task CriarPedidoAjuda_ValidData_ReturnsOk()
        {

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


            var result = await _controller.CriarPedidoAjuda(dto);


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


        /// <summary>
        /// Testa o comportamento do método CriarPedidoAjuda quando os dados fornecidos são inválidos.
        /// Espera-se que o método retorne um erro de BadRequest (status 400) com a mensagem "Dados inválidos".
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task CriarPedidoAjuda_InvalidData_ReturnsBadRequest()
        {

            var pedidoData = new PedidoAjudaDTO
            {
                DescPedido = "",
                NHoras = 0,
                NPessoas = 0,
                HorarioAjuda = DateTime.Now.AddDays(-1),
                FotografiaPA = ""
            };


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


            var result = await _controller.CriarPedidoAjuda(pedidoData);


            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var actionResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Dados inválidos.", actionResult.Value);
        }

        #endregion


        #region RejeitarPedido

        /// <summary>
        /// Testa o comportamento do método RejeitarPedidoAjuda quando o administrador valida um pedido de ajuda com sucesso.
        /// Espera-se que o método retorne um status 200 OK com a mensagem "Pedido de ajuda rejeitado com sucesso."
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task RejeitarPedidoAjuda_ValidAdmin_ReturnsOk()
        {

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


        /// <summary>
        /// Testa o comportamento do método RejeitarPedidoAjuda quando um utilizador não autorizado tenta rejeitar um pedido de ajuda.
        /// Espera-se que o método retorne um erro de "não autorizado" (status 403) quando o utilizador não for um administrador.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task RejeitarPedidoAjuda_Unauthorized_ReturnsUnauthorized()
        {

            var pedidoId = 1;
            var user = new Utilizador
            {
                UtilizadorId = 1,
                TipoUtilizadorId = 1,
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


            var result = await _controller.RejeitarPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }


        /// <summary>
        /// Testa o comportamento do método RejeitarPedidoAjuda quando o pedido de ajuda não é encontrado na base de dados.
        /// Espera-se que o método retorne um erro de "não encontrado" (status 404) se o pedido de ajuda não existir.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task RejeitarPedidoAjuda_PedidoNaoEncontrado_ReturnsNotFound()
        {

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


            var result = await _controller.RejeitarPedidoAjuda(pedidoIdInexistente);


            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFound = (NotFoundObjectResult)result;
            Assert.AreEqual("Pedido de ajuda não encontrado.", notFound.Value);
        }


        #endregion


        #region ValidarPedido

        /// <summary>
        /// Testa o comportamento do método ValidarPedidoAjuda quando o utilizador não está autenticado.
        /// Espera-se que o método retorne um erro de "não autorizado" (401) se o utilizador não estiver autenticado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarPedidoAjuda_InvalidUser_ReturnsUnauthorized()
        {

            var userIdentity = new ClaimsIdentity();
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };


            var result = await _controller.ValidarPedidoAjuda(1);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var actionResult = (UnauthorizedObjectResult)result;
            Assert.AreEqual("Utilizador não autenticado.", actionResult.Value);
        }


        /// <summary>
        /// Testa o comportamento do método ValidarPedidoAjuda quando o utilizador não é um administrador.
        /// Espera-se que o método retorne um erro de "proibido" (403) se o utilizador não for um administrador.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarPedidoAjuda_NotAdminUser_ReturnsForbid()
        {

            var pedidoId = 1;
            var user = new Utilizador { UtilizadorId = 2, TipoUtilizadorId = 1 };


            _mockUtilizadoresDbSet.Setup(m => m.FindAsync(2)).ReturnsAsync(user);


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


            var result = await _controller.ValidarPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }


        /// <summary>
        /// Testa o comportamento do método ValidarPedidoAjuda quando o pedido de ajuda não é encontrado.
        /// Espera-se que o método retorne um erro de "não encontrado" (404) se o pedido de ajuda não existir.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarPedidoAjuda_PedidoNaoEncontrado_ReturnsNotFound()
        {

            var pedidoId = 999;
            var user = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 2 };


            _mockUtilizadoresDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(user);
            _mockPedidosAjudaDbSet.Setup(m => m.FindAsync(pedidoId)).ReturnsAsync((PedidoAjuda)null);


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


            var result = await _controller.ValidarPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var actionResult = (NotFoundObjectResult)result;
            Assert.AreEqual("Pedido de ajuda não encontrado.", actionResult.Value);
        }


        /// <summary>
        /// Testa o comportamento do método ValidarPedidoAjuda quando o pedido de ajuda já foi validado.
        /// Espera-se que o método retorne um erro de "má solicitação" (400) se o pedido já foi validado ou está em andamento/concluído.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarPedidoAjuda_PedidoJaValidado_ReturnsBadRequest()
        {

            var pedidoId = 1;
            var user = new Utilizador { UtilizadorId = 1, TipoUtilizadorId = 2 };
            var pedido = new PedidoAjuda { PedidoId = pedidoId, Estado = EstadoPedido.Aberto, UtilizadorId = 1 };


            _mockUtilizadoresDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(user);
            _mockPedidosAjudaDbSet.Setup(m => m.FindAsync(pedidoId)).ReturnsAsync(pedido);


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


            var result = await _controller.ValidarPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var actionResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Este pedido já foi validado ou está em progresso/concluído.", actionResult.Value);
        }


        /// <summary>
        /// Testa o comportamento do método ValidarPedidoAjuda quando o pedido é validado com sucesso.
        /// Espera-se que o método retorne um status 200 OK com a mensagem "Pedido de ajuda validado com sucesso e colocado como 'Aberto'."
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarPedidoAjuda_Success_ReturnsOk()
        {

            var pedidoId = 1;

            var admin = new Utilizador
            {
                UtilizadorId = 1,
                NomeUtilizador = "Admin",
                TipoUtilizadorId = 2
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


            var result = await _controller.ValidarPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(
                "Pedido de ajuda validado com sucesso e colocado como 'Aberto'.",
                ok.Value);
        }


        #endregion


        #region Voluntariar


        /// <summary>
        /// Testa o comportamento do método Voluntariar quando o voluntariado é realizado com sucesso.
        /// Espera-se que o método retorne uma resposta de sucesso (200 OK) com a mensagem de que o pedido de voluntariado foi registado com sucesso e aguarda aprovação do administrador.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task Voluntariar_Success_ReturnsOk()
        {

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


            var result = await _controller.Voluntariar(pedidoId);


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = result as OkObjectResult;
            Assert.AreEqual("Pedido de voluntariado registado com sucesso. Aguardando aprovação do administrador.", ok.Value);
        }


        /// <summary>
        /// Testa o comportamento do método Voluntariar quando o pedido está em progresso.
        /// Espera-se que o método retorne um erro de "bad request" (400), com a mensagem de que o pedido não foi encontrado ou já foi fechado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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


        /// <summary>
        /// Testa o comportamento do método Voluntariar quando o número máximo de voluntários já foi atingido.
        /// Espera-se que o método retorne um erro de "bad request" (400), com a mensagem indicando que o número máximo de voluntários foi atingido para o pedido.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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


        /// <summary>
        /// Testa o comportamento do método Voluntariar quando o utilizador já se voluntariou para o pedido.
        /// Espera-se que o método retorne um erro de "bad request" (400), com a mensagem indicando que o utilizador já se voluntariou para o pedido.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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

        /// <summary>
        /// Testa o comportamento do método ConcluirPedidoAjuda quando um utilizador válido conclui um pedido de ajuda.
        /// Espera-se que o pedido seja marcado como concluído e que os administradores sejam notificados para validar a conclusão.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ConcluirPedidoAjuda_ValidUser_ReturnsOk()
        {

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


            var result = await _controller.ConcluirPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(
                "O pedido foi marcado como concluído. Os administradores foram notificados para validar a conclusão.",
                ok.Value);


            Assert.AreEqual(EstadoPedido.Concluido, pedido.Estado);


            notificacoesDbSet.Verify(d => d.Add(It.IsAny<Notificacao>()), Times.Once);


            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        /// <summary>
        /// Testa o comportamento do método ConcluirPedidoAjuda quando o utilizador não está autenticado.
        /// Espera-se que o método retorne um erro de "não autorizado" (erro 401) se o utilizador não estiver autenticado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>


        [TestMethod]
        public async Task ConcluirPedidoAjuda_UserNotAuthenticated_ReturnsUnauthorized()
        {

            var pedidoId = 1;
            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.EmProgresso,
                UtilizadorId = 2
            };


            var mockDbSetPedidos = new Mock<DbSet<PedidoAjuda>>();
            mockDbSetPedidos.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(pedido);
            _mockContext.Setup(c => c.PedidosAjuda).Returns(mockDbSetPedidos.Object);


            var userClaims = new List<Claim>();
            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };


            var result = await _controller.ConcluirPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var actionResult = (UnauthorizedObjectResult)result;
            Assert.AreEqual("Utilizador não autenticado.", actionResult.Value);
        }


        /// <summary>
        /// Testa o comportamento do método ConcluirPedidoAjuda quando o pedido não é encontrado no banco de dados.
        /// Espera-se que o método retorne um erro 404 quando o pedido de ajuda não existir.
        /// </summary>
        /// <returns>1 resultado - Uma tarefa que representa a execução do teste unitário.</returns>

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


        /// <summary>
        /// Testa o comportamento do método ConcluirPedidoAjuda quando o utilizador não é o requerente do pedido.
        /// Espera-se que o método retorne um erro de "proibido" (erro 403) se o utilizador não for o requerente do pedido.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ConcluirPedidoAjuda_UserIsNotRequester_ReturnsForbid()
        {

            var pedidoId = 1;

            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.EmProgresso,
                UtilizadorId = 3
            };


            var pedidosDbSet = new[] { pedido }
                               .AsQueryable()
                               .BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda)
                        .Returns(pedidosDbSet.Object);


            var user = new ClaimsPrincipal(
                           new ClaimsIdentity(
                               new[] { new Claim(ClaimTypes.NameIdentifier, "2") }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };


            var result = await _controller.ConcluirPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(ForbidResult));

        }


        /// <summary>
        /// Testa o comportamento do método ConcluirPedidoAjuda quando o pedido não está em progresso.
        /// Espera-se que o método retorne um erro de "requisição inválida" (erro 400) se o pedido já estiver concluído ou não estiver em progresso.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ConcluirPedidoAjuda_PedidoNotInProgress_ReturnsBadRequest()
        {

            var pedidoId = 1;

            var pedido = new PedidoAjuda
            {
                PedidoId = pedidoId,
                Estado = EstadoPedido.Concluido,
                UtilizadorId = 2
            };


            var pedidosDbSet = new[] { pedido }
                               .AsQueryable()
                               .BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda)
                        .Returns(pedidosDbSet.Object);


            var user = new ClaimsPrincipal(
                           new ClaimsIdentity(
                               new[] { new Claim(ClaimTypes.NameIdentifier, "2") }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };


            var result = await _controller.ConcluirPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("O pedido não está em progresso ou já foi concluído.", bad.Value);
        }

        #endregion


        #region ValidarConclusao

        /// <summary>
        /// Testa o comportamento do método ValidarConclusaoPedidoAjuda quando o utilizador não está autenticado.
        /// Espera-se que o método devolva uma resposta 401 Unauthorized com a mensagem correspondente.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_UserNotAuthenticated_ReturnsUnauthorized()
        {

            var pedidoId = 1;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };


            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var objectResult = (UnauthorizedObjectResult)result;
            Assert.AreEqual("Utilizador não autenticado.", objectResult.Value);
        }


        /// <summary>
        /// Testa o comportamento do método ValidarConclusaoPedidoAjuda quando o pedido de ajuda especifico não é encontrado.
        /// Espera-se que o método devolva uma resposta 404 NotFound com a mensagem apropriada.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_PedidoNotFound_ReturnsNotFound()
        {

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


            var pedidos = new List<PedidoAjuda>()
                          .AsQueryable()
                          .BuildMockDbSet();

            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidos.Object);


            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFound = (NotFoundObjectResult)result;
            Assert.AreEqual("Pedido de ajuda não encontrado.", notFound.Value);
        }


        /// <summary>
        /// Testa o comportamento do método ValidarConclusaoPedidoAjuda quando o utilizador autenticado não é um administrador.
        /// Espera-se que o método devolva uma resposta 403 Forbid, impedindo o acesso à funcionalidade.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_UserIsNotAdmin_ReturnsForbid()
        {

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
                .ReturnsAsync(new Utilizador { UtilizadorId = utilizadorId, TipoUtilizadorId = 1 });

            _mockContext.Setup(c => c.Utilizadores).Returns(mockDbSetUtilizadores.Object);


            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }


        /// <summary>
        /// Testa o comportamento do método ValidarConclusaoPedidoAjuda quando o pedido de ajuda não foi concluído.
        /// Espera-se que o método devolva uma resposta 400 BadRequest com a mensagem correspondente.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_PedidoNotConcluded_ReturnsBadRequest()
        {

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


            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("O pedido não ainda não foi concluído.", bad.Value);
        }


        /// <summary>
        /// Testa o comportamento do método ValidarConclusaoPedidoAjuda quando o recetor do pedido é nulo.
        /// Espera-se que o método devolva uma resposta 400 BadRequest com a mensagem de erro correspondente.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_RecetorIsNull_ReturnsBadRequest()
        {

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


            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);


            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("Não foi possível determinar o recetor do pedido.", bad.Value);
        }


        /// <summary>
        /// Testa o cenário completo e bem-sucedido em que um administrador valida a conclusão de um pedido de ajuda.
        /// Garante que a recompensa é atribuída, a transação é registada e a notificação é enviada corretamente.
        /// </summary>
        /// <returns>
        /// Resultado 200 (OK) com mensagem de sucesso. Verifica também alterações no número de cares do recetor
        /// e chamadas aos métodos Add e SaveChangesAsync para persistência dos dados.
        /// </returns>

        [TestMethod]
        public async Task ValidarConclusaoPedidoAjuda_ValidRequest_ReturnsOk()
        {

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


            var result = await _controller.ValidarConclusaoPedidoAjuda(pedidoId);


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

        /// <summary>
        /// Testa o comportamento do método GetPedidosAjudaDisponiveis quando o utilizador não está autenticado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task GetPedidosAjudaDisponiveis_UnauthenticatedUser_ReturnsUnauthorized()
        {

            var userIdentity = new ClaimsIdentity();
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };


            var result = await _controller.GetPedidosAjudaDisponiveis();


            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
            var actionResult = result.Result as UnauthorizedObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual("Utilizador não autenticado.", actionResult.Value);
        }


        /// <summary>
        /// Testa o comportamento do método GetPedidosAjudaDisponiveis quando o utilizador está autenticado
        /// e existem pedidos de ajuda disponíveis (abertos) de outros utilizadores.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task GetPedidosAjudaDisponiveis_Success_ReturnsPedidosDisponiveis()
        {

            const int currentUserId = 1;

            var pedidos = new List<PedidoAjuda>
            {
                new PedidoAjuda { PedidoId = 1, UtilizadorId = 2, Estado = EstadoPedido.Aberto },
                new PedidoAjuda { PedidoId = 2, UtilizadorId = 3, Estado = EstadoPedido.Aberto },

                new PedidoAjuda { PedidoId = 3, UtilizadorId = currentUserId, Estado = EstadoPedido.Aberto },
                new PedidoAjuda { PedidoId = 4, UtilizadorId = 9, Estado = EstadoPedido.Pendente }
            };


            var pedidosDbSet = pedidos.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidosDbSet.Object);


            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                               new ClaimsIdentity(
                                   new[] { new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString()) },
                                   "TestAuth"))
                }
            };


            var result = await _controller.GetPedidosAjudaDisponiveis();


            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result.Result;

            var returned = ok.Value as IEnumerable<PedidoAjuda>;
            Assert.IsNotNull(returned);

            var list = returned.ToList();
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.All(p => p.Estado == EstadoPedido.Aberto
                                     && p.UtilizadorId != currentUserId));
        }

        #endregion


        #region GetMeusPedidosAjuda

        /// <summary>
        /// Testa o método GetMeusPedidosAjuda do controlador quando o utilizador não está autenticado.
        /// Verifica se o resultado devolvido é do tipo UnauthorizedObjectResult e se a mensagem de erro está correta.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task GetMeusPedidosAjuda_UnauthenticatedUser_ReturnsUnauthorized()
        {

            var userIdentity = new ClaimsIdentity();
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };


            var result = await _controller.GetMeusPedidosAjuda();


            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
            var actionResult = result.Result as UnauthorizedObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual("Utilizador não autenticado.", actionResult.Value);
        }


        /// <summary>
        /// Testa o comportamento do método GetMeusPedidosAjuda quando o utilizador está autenticado
        /// e existem pedidos associados ao seu identificador.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task GetMeusPedidosAjuda_Success_ReturnsMeusPedidos()
        {

            const int userId = 1;

            var pedidos = new List<PedidoAjuda>
            {
                new PedidoAjuda { PedidoId = 1, UtilizadorId = userId, Estado = EstadoPedido.Pendente },
                new PedidoAjuda { PedidoId = 2, UtilizadorId = userId, Estado = EstadoPedido.Pendente },
                new PedidoAjuda { PedidoId = 3, UtilizadorId = 99,   Estado = EstadoPedido.Pendente }
            };


            var pedidosDbSet = pedidos.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.PedidosAjuda).Returns(pedidosDbSet.Object);


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


            var result = await _controller.GetMeusPedidosAjuda();


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