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
using MockQueryable.Moq;


namespace CommuniCareTests
{
    [TestClass]
    public class ItensEmprestimoTests
    {
        private ItensEmprestimoController _controller;
        private Mock<CommuniCareContext> _mockContext;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CommuniCareContext>();

            _controller = new ItensEmprestimoController(_mockContext.Object);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task AdicionarItemEmprestimo_ValidData_ReturnsOk()
        {
            // Arrange
            var itemData = new ItemEmprestimoDTO
            {
                NomeItem = "Martelo",
                DescItem = "Martelo de ferro",
                ComissaoCares = 20,
                FotografiaItem = "martelo.jpg"
            };

            var utilizador = new Utilizador
            {
                UtilizadorId = 1,
                TipoUtilizadorId = 1,
                NomeUtilizador = "Carlos"
            };

            var admin = new Utilizador
            {
                UtilizadorId = 2,
                TipoUtilizadorId = 2,
                NomeUtilizador = "Admin"
            };

            var utilizadoresList = new List<Utilizador> { utilizador, admin }.AsQueryable();
            var mockUtilizadoresDbSet = utilizadoresList.BuildMockDbSet();

            var mockItensEmprestimoDbSet = new Mock<DbSet<ItemEmprestimo>>();
            var mockRelacoesDbSet = new Mock<DbSet<ItemEmprestimoUtilizador>>();
            var mockNotificacoesDbSet = new Mock<DbSet<Notificacao>>();

            _mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresDbSet.Object);
            _mockContext.Setup(c => c.ItensEmprestimo).Returns(mockItensEmprestimoDbSet.Object);
            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores).Returns(mockRelacoesDbSet.Object);
            _mockContext.Setup(c => c.Notificacaos).Returns(mockNotificacoesDbSet.Object);

            _mockContext.Setup(c => c.Utilizadores.FindAsync(1)).ReturnsAsync(utilizador);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Simula utilizador autenticado
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.AdicionarItemEmprestimo(itemData);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Item de empréstimo adicionado com sucesso. Aguardando validação.", okResult.Value);
        }


        [TestMethod]
        public async Task AdicionarItemEmprestimo_InvalidData_ReturnsUnauthorized()
        {
            // Arrange
            var itemData = new ItemEmprestimoDTO
            {
                NomeItem = "Martelo",
                DescItem = "Martelo de ferro",
                ComissaoCares = 20,
                FotografiaItem = "martelo.jpg"
            };

            // Sem Claims (usuário não autenticado)
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = await _controller.AdicionarItemEmprestimo(itemData);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorizedResult.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task AdquirirItem_ValidData_ReturnsOk()
        {
            // Arrange
            var utilizador = new Utilizador
            {
                UtilizadorId = 1,
                NomeUtilizador = "Carlos",
                TipoUtilizadorId = 1,
                NumCares = 100
            };

            var item = new ItemEmprestimo
            {
                ItemId = 10,
                NomeItem = "Martelo",
                Disponivel = 1,
                ComissaoCares = 10,
                Utilizadores = new List<Utilizador>
                {
                    new Utilizador { UtilizadorId = 1, NomeUtilizador = "Carlos" }
                }
            };

            var admins = new List<Utilizador>
            {
                new Utilizador { UtilizadorId = 2, TipoUtilizadorId = 2, NomeUtilizador = "Admin" }
            }.AsQueryable();

            var relacoes = new List<ItemEmprestimoUtilizador>().AsQueryable();

            var mockItensEmprestimoDbSet = new List<ItemEmprestimo> { item }.AsQueryable().BuildMockDbSet();
            var mockUtilizadoresDbSet = new List<Utilizador> { utilizador, admins.First() }.AsQueryable().BuildMockDbSet();
            var mockRelacoesDbSet = relacoes.BuildMockDbSet();
            var mockEmprestimosDbSet = new Mock<DbSet<Emprestimo>>();
            var mockNotificacoesDbSet = new Mock<DbSet<Notificacao>>();

            _mockContext.Setup(c => c.ItensEmprestimo).Returns(mockItensEmprestimoDbSet.Object);
            _mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresDbSet.Object);
            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores).Returns(mockRelacoesDbSet.Object);
            _mockContext.Setup(c => c.Emprestimos).Returns(mockEmprestimosDbSet.Object);
            _mockContext.Setup(c => c.Notificacaos).Returns(mockNotificacoesDbSet.Object);
            _mockContext.Setup(c => c.Utilizadores.FindAsync(1)).ReturnsAsync(utilizador);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Simula utilizador autenticado
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.AdquirirItem(10);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Pedido de empréstimo efetuado. Aguarde validação do administrador.", okResult.Value);
        }

        [TestMethod]
        public async Task AdquirirItem_InvalidData_ReturnsBadRequest_SaldoInsuficiente()
        {
            // Arrange
            var utilizador = new Utilizador
            {
                UtilizadorId = 1,
                NomeUtilizador = "Carlos",
                TipoUtilizadorId = 1,
                NumCares = 5 // saldo insuficiente
            };

            var item = new ItemEmprestimo
            {
                ItemId = 10,
                NomeItem = "Martelo",
                Disponivel = 1,
                ComissaoCares = 10,
                Utilizadores = new List<Utilizador>()
            };

            var admins = new List<Utilizador>
            {
                new Utilizador { UtilizadorId = 2, TipoUtilizadorId = 2, NomeUtilizador = "Admin" }
            }.AsQueryable();

            var relacoes = new List<ItemEmprestimoUtilizador>().AsQueryable();

            var mockItensEmprestimoDbSet = new List<ItemEmprestimo> { item }.AsQueryable().BuildMockDbSet();
            var mockUtilizadoresDbSet = new List<Utilizador> { utilizador, admins.First() }.AsQueryable().BuildMockDbSet();
            var mockRelacoesDbSet = relacoes.BuildMockDbSet();
            var mockEmprestimosDbSet = new Mock<DbSet<Emprestimo>>();
            var mockNotificacoesDbSet = new Mock<DbSet<Notificacao>>();

            _mockContext.Setup(c => c.ItensEmprestimo).Returns(mockItensEmprestimoDbSet.Object);
            _mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadoresDbSet.Object);
            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores).Returns(mockRelacoesDbSet.Object);
            _mockContext.Setup(c => c.Emprestimos).Returns(mockEmprestimosDbSet.Object);
            _mockContext.Setup(c => c.Notificacaos).Returns(mockNotificacoesDbSet.Object);
            _mockContext.Setup(c => c.Utilizadores.FindAsync(1)).ReturnsAsync(utilizador);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Simula utilizador autenticado
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.AdquirirItem(10);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("Saldo de Cares insuficiente para adquirir este item.", badRequest.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task GetItensDisponiveis_ValidData_ReturnsAvailableItems()
        {
            // Arrange
            int userId = 1;

            var relacoes = new List<ItemEmprestimoUtilizador>
            {
                new ItemEmprestimoUtilizador { UtilizadorId = userId, ItemId = 1, TipoRelacao = "Dono" }
            }.AsQueryable();

            var itens = new List<ItemEmprestimo>
            {
             new ItemEmprestimo { ItemId = 2, NomeItem = "Martelo", Disponivel = 1 },
             new ItemEmprestimo { ItemId = 1, NomeItem = "Chave", Disponivel = 1 } // do usuário, deve ser excluído
            }.AsQueryable();

            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores).Returns(relacoes.BuildMockDbSet().Object);
            _mockContext.Setup(c => c.ItensEmprestimo).Returns(itens.BuildMockDbSet().Object);

            // Simula autenticação
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };

            // Act
            var result = await _controller.GetItensDisponiveis();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            var itensDisponiveis = okResult.Value as IEnumerable<ItemEmprestimo>;
            Assert.AreEqual(1, itensDisponiveis.Count()); // só o itemId 2
        }

        [TestMethod]
        public async Task GetItensDisponiveis_InvalidUser_ReturnsUnauthorized()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal() // sem identidade
                }
            };

            // Act
            var result = await _controller.GetItensDisponiveis();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
            var unauthorized = result.Result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorized.Value);
        }

        [TestMethod]
        public async Task GetMeusItens_ValidData_ReturnsOwnedItems()
        {
            // Arrange
            int userId = 1;

            var relacoes = new List<ItemEmprestimoUtilizador>
            {
                 new ItemEmprestimoUtilizador { UtilizadorId = userId, ItemId = 1, TipoRelacao = "Dono" }
            }.AsQueryable();

            var itens = new List<ItemEmprestimo>
            {
             new ItemEmprestimo { ItemId = 1, NomeItem = "Chave", Disponivel = 1 },
             new ItemEmprestimo { ItemId = 2, NomeItem = "Martelo", Disponivel = 1 }
            }.AsQueryable();

            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores).Returns(relacoes.BuildMockDbSet().Object);
            _mockContext.Setup(c => c.ItensEmprestimo).Returns(itens.BuildMockDbSet().Object);

            // Simula autenticação
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };

            // Act
            var result = await _controller.GetMeusItens();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            var meusItens = okResult.Value as IEnumerable<ItemEmprestimo>;
            Assert.AreEqual(1, meusItens.Count());
            Assert.AreEqual(1, meusItens.First().ItemId);
        }


        [TestMethod]
        public async Task GetMeusItens_InvalidUser_ReturnsUnauthorized()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal() // sem claims
                }
            };

            // Act
            var result = await _controller.GetMeusItens();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
            var unauthorized = result.Result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorized.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task EliminarItemEmprestimo_ValidOwner_ReturnsNoContent()
        {
            int userId = 1;
            int itemId = 10;

            var item = new ItemEmprestimo { ItemId = itemId, Disponivel = 1 };
            var emprestimo = new Emprestimo { EmprestimoId = 1, Items = new List<ItemEmprestimo> { item } };
            var relacao = new ItemEmprestimoUtilizador { ItemId = itemId, UtilizadorId = userId, TipoRelacao = "Dono" };

            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo> { item }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.Emprestimos)
                .Returns(new List<Emprestimo> { emprestimo }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores)
                .Returns(new List<ItemEmprestimoUtilizador> { relacao }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuth"))
                }
            };

            var result = await _controller.EliminarItemEmprestimo(itemId);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task EliminarItemEmprestimo_Unauthenticated_ReturnsUnauthorized()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal() // no identity
                }
            };

            var result = await _controller.EliminarItemEmprestimo(10);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }


        [TestMethod]
        public async Task EliminarItemEmprestimo_ItemNotFound_ReturnsNotFound()
        {
            int userId = 1;

            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo>().AsQueryable().BuildMockDbSet().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuth"))
                }
            };

            var result = await _controller.EliminarItemEmprestimo(10);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFound = result as NotFoundObjectResult;
            Assert.AreEqual("Item de empréstimo não encontrado.", notFound.Value);
        }


        [TestMethod]
        public async Task EliminarItemEmprestimo_EmprestimoNotFound_ReturnsNotFound()
        {
            int userId = 1;
            int itemId = 10;
            var item = new ItemEmprestimo { ItemId = itemId };

            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo> { item }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.Emprestimos)
                .Returns(new List<Emprestimo>().AsQueryable().BuildMockDbSet().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuth"))
                }
            };

            var result = await _controller.EliminarItemEmprestimo(itemId);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFound = result as NotFoundObjectResult;
            Assert.AreEqual("Empréstimo relacionado não encontrado.", notFound.Value);
        }


        [TestMethod]
        public async Task EliminarItemEmprestimo_NotOwner_ReturnsUnauthorized()
        {
            int userId = 1;
            int itemId = 10;
            var item = new ItemEmprestimo { ItemId = itemId };
            var emprestimo = new Emprestimo { Items = new List<ItemEmprestimo> { item } };
            var relacao = new ItemEmprestimoUtilizador
            {
                ItemId = itemId,
                UtilizadorId = userId,
                TipoRelacao = "Comprador"
            };

            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo> { item }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.Emprestimos)
                .Returns(new List<Emprestimo> { emprestimo }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores)
                .Returns(new List<ItemEmprestimoUtilizador> { relacao }.AsQueryable().BuildMockDbSet().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuth"))
                }
            };

            var result = await _controller.EliminarItemEmprestimo(itemId);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.AreEqual("Você não tem permissão para alterar este item.", unauthorized.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task AtualizarDescricaoItem_ValidOwner_ReturnsNoContent()
        {
            int userId = 1;
            int itemId = 10;
            string novaDescricao = "Nova descrição do martelo";

            var item = new ItemEmprestimo { ItemId = itemId, DescItem = "Descrição antiga", Disponivel = 1 };
            var emprestimo = new Emprestimo { EmprestimoId = 1, Items = new List<ItemEmprestimo> { item } };
            var relacao = new ItemEmprestimoUtilizador { ItemId = itemId, UtilizadorId = userId, TipoRelacao = "Dono" };

            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo> { item }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.Emprestimos)
                .Returns(new List<Emprestimo> { emprestimo }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores)
                .Returns(new List<ItemEmprestimoUtilizador> { relacao }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuth"))
                }
            };

            var result = await _controller.AtualizarDescricaoItem(itemId, novaDescricao);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task AtualizarDescricaoItem_Unauthenticated_ReturnsUnauthorized()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal() // Sem identidade
                }
            };

            var result = await _controller.AtualizarDescricaoItem(10, "Nova descrição");

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorized.Value);
        }

        [TestMethod]
        public async Task AtualizarDescricaoItem_InvalidUserId_ReturnsUnauthorized()
        {
            int itemId = 10;
            string novaDescricao = "Nova descrição";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, "invalid_id")
                    }, "TestAuth"))
                }
            };

            var result = await _controller.AtualizarDescricaoItem(itemId, novaDescricao);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.AreEqual("ID de utilizador inválido.", unauthorized.Value);
        }

        [TestMethod]
        public async Task AtualizarDescricaoItem_ItemNotFound_ReturnsNotFound()
        {
            int userId = 1;
            int itemId = 10;
            string novaDescricao = "Nova descrição";

            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo>().AsQueryable().BuildMockDbSet().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuth"))
                }
            };

            var result = await _controller.AtualizarDescricaoItem(itemId, novaDescricao);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFound = result as NotFoundObjectResult;
            Assert.AreEqual("Item de empréstimo não encontrado.", notFound.Value);
        }

        [TestMethod]
        public async Task AtualizarDescricaoItem_EmprestimoNotFound_ReturnsNotFound()
        {
            int userId = 1;
            int itemId = 10;
            string novaDescricao = "Nova descrição";

            var item = new ItemEmprestimo { ItemId = itemId };

            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo> { item }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.Emprestimos)
                .Returns(new List<Emprestimo>().AsQueryable().BuildMockDbSet().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuth"))
                }
            };

            var result = await _controller.AtualizarDescricaoItem(itemId, novaDescricao);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFound = result as NotFoundObjectResult;
            Assert.AreEqual("Empréstimo relacionado não encontrado.", notFound.Value);
        }

        [TestMethod]
        public async Task AtualizarDescricaoItem_NotOwner_ReturnsUnauthorized()
        {
            int userId = 1;
            int itemId = 10;
            string novaDescricao = "Nova descrição";

            var item = new ItemEmprestimo { ItemId = itemId };
            var emprestimo = new Emprestimo { Items = new List<ItemEmprestimo> { item } };
            var relacao = new ItemEmprestimoUtilizador
            {
                ItemId = itemId,
                UtilizadorId = userId,
                TipoRelacao = "Comprador"
            };

            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo> { item }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.Emprestimos)
                .Returns(new List<Emprestimo> { emprestimo }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores)
                .Returns(new List<ItemEmprestimoUtilizador> { relacao }.AsQueryable().BuildMockDbSet().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuth"))
                }
            };

            var result = await _controller.AtualizarDescricaoItem(itemId, novaDescricao);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.AreEqual("Você não tem permissão para atualizar este item.", unauthorized.Value);
        }


        /// <summary>
        /// Administrador
        /// </summary>
        /// <returns></returns>
        /// 

        [TestMethod]
        public async Task ValidarItemEmprestimo_ValidAdmin_ReturnsOk()
        {
            int adminId = 1;
            int itemId = 10;

            var item = new ItemEmprestimo { ItemId = itemId, NomeItem = "Martelo", Disponivel = 0 };
            var admin = new Utilizador { UtilizadorId = adminId, TipoUtilizadorId = 2 };

            var relacaoDono = new ItemEmprestimoUtilizador { ItemId = itemId, UtilizadorId = 1, TipoRelacao = "Dono" };
            var notificacao = new Notificacao { UtilizadorId = 1, ItemId = itemId, Mensagem = "Item validado", Lida = 0, DataMensagem = DateTime.Now };

            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo> { item }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.Utilizadores.FindAsync(adminId)).ReturnsAsync(admin);
            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores)
                .Returns(new List<ItemEmprestimoUtilizador> { relacaoDono }.AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.Notificacaos.Add(It.IsAny<Notificacao>()));

            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
                    }, "TestAuth"))
                }
            };

            var result = await _controller.ValidarItemEmprestimo(itemId);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Item de empréstimo validado e tornado disponível com sucesso.", okResult.Value);
        }


        [TestMethod]
        public async Task ValidarItemEmprestimo_Unauthorized_ReturnsUnauthorized()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal() // Sem identidade
                }
            };

            var result = await _controller.ValidarItemEmprestimo(10);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }


        [TestMethod]
        public async Task ValidarItemEmprestimo_NotAdmin_ReturnsForbid()
        {
            int userId = 1;
            int itemId = 10;

            var user = new Utilizador { UtilizadorId = userId, TipoUtilizadorId = 1 }; // Não é admin

            _mockContext.Setup(c => c.Utilizadores.FindAsync(userId)).ReturnsAsync(user);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuth"))
                }
            };

            var result = await _controller.ValidarItemEmprestimo(itemId);

            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        [TestMethod]
        public async Task ValidarItemEmprestimo_ItemNotFound_ReturnsNotFound()
        {
            int adminId = 1;
            int itemId = 10;

            var admin = new Utilizador { UtilizadorId = adminId, TipoUtilizadorId = 2 };

            _mockContext.Setup(c => c.Utilizadores.FindAsync(adminId)).ReturnsAsync(admin);
            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo>().AsQueryable().BuildMockDbSet().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
                    }, "TestAuth"))
                }
            };

            var result = await _controller.ValidarItemEmprestimo(itemId);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFound = result as NotFoundObjectResult;
            Assert.AreEqual("Item não encontrado.", notFound.Value);
        }


        [TestMethod]
        public async Task ValidarItemEmprestimo_ItemAlreadyValidated_ReturnsBadRequest()
        {
            int adminId = 1;
            int itemId = 10;

            var item = new ItemEmprestimo { ItemId = itemId, NomeItem = "Martelo", Disponivel = 1 }; // Já está disponível
            var admin = new Utilizador { UtilizadorId = adminId, TipoUtilizadorId = 2 };

            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo> { item }.AsQueryable().BuildMockDbSet().Object);

            _mockContext.Setup(c => c.Utilizadores.FindAsync(adminId)).ReturnsAsync(admin);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                     new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
                    }, "TestAuth"))
                }
            };

            var result = await _controller.ValidarItemEmprestimo(itemId);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("Este item já está validado e disponível.", badRequest.Value);
        }


        /// <summary>
        /// Administrador
        /// </summary>
        /// <returns></returns>
        /// 

        [TestMethod]
        public async Task RejeitarItemEmprestimo_AdminValid_ReturnsOk()
        {
            int adminId = 1;
            int itemId = 10;
            int donoId = 2;

            var item = new ItemEmprestimo { ItemId = itemId, NomeItem = "Livro" };
            var admin = new Utilizador { UtilizadorId = adminId, TipoUtilizadorId = 2 };
            var relacaoDono = new ItemEmprestimoUtilizador { ItemId = itemId, UtilizadorId = donoId, TipoRelacao = "Dono" };

            var notificacoes = new List<Notificacao> {
        new Notificacao { NotificacaoId = 1, ItemId = itemId }
    };

            _mockContext.Setup(c => c.Utilizadores.FindAsync(adminId)).ReturnsAsync(admin);
            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo> { item }.AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores)
                .Returns(new List<ItemEmprestimoUtilizador> { relacaoDono }.AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.Notificacaos)
                .Returns(notificacoes.AsQueryable().BuildMockDbSet().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
            }, "TestAuth"))
                }
            };

            var result = await _controller.RejeitarItemEmprestimo(itemId);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual("Item rejeitado e removido com sucesso.", (result as OkObjectResult)?.Value);
        }


        [TestMethod]
        public async Task RejeitarItemEmprestimo_Unauthorized_ReturnsUnauthorized()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal() // No identity
                }
            };

            var result = await _controller.RejeitarItemEmprestimo(10);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }


        [TestMethod]
        public async Task RejeitarItemEmprestimo_NotAdmin_ReturnsForbid()
        {
            int userId = 1;
            var user = new Utilizador { UtilizadorId = userId, TipoUtilizadorId = 1 }; // Not admin

            _mockContext.Setup(c => c.Utilizadores.FindAsync(userId)).ReturnsAsync(user);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "TestAuth"))
                }
            };

            var result = await _controller.RejeitarItemEmprestimo(10);

            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }


        [TestMethod]
        public async Task RejeitarItemEmprestimo_ItemNotFound_ReturnsNotFound()
        {
            int adminId = 1;
            var admin = new Utilizador { UtilizadorId = adminId, TipoUtilizadorId = 2 };

            _mockContext.Setup(c => c.Utilizadores.FindAsync(adminId)).ReturnsAsync(admin);
            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo>().AsQueryable().BuildMockDbSet().Object); // Empty list

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
            }, "TestAuth"))
                }
            };

            var result = await _controller.RejeitarItemEmprestimo(10);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Item não encontrado.", (result as NotFoundObjectResult)?.Value);
        }


        [TestMethod]
        public async Task RejeitarItemEmprestimo_ItemSemDono_StillSucceeds()
        {
            int adminId = 1;
            int itemId = 10;

            var item = new ItemEmprestimo { ItemId = itemId, NomeItem = "Martelo" };
            var admin = new Utilizador { UtilizadorId = adminId, TipoUtilizadorId = 2 };

            _mockContext.Setup(c => c.Utilizadores.FindAsync(adminId)).ReturnsAsync(admin);
            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo> { item }.AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores)
                .Returns(new List<ItemEmprestimoUtilizador>().AsQueryable().BuildMockDbSet().Object); // Sem dono
            _mockContext.Setup(c => c.Notificacaos)
                .Returns(new List<Notificacao>().AsQueryable().BuildMockDbSet().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
            }, "TestAuth"))
                }
            };

            var result = await _controller.RejeitarItemEmprestimo(itemId);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual("Item rejeitado e removido com sucesso.", (result as OkObjectResult)?.Value);
        }




    }
}
