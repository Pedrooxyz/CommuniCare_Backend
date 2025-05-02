/// <summary>
/// Namespace que contém os testes unitários da aplicação CommuniCare.
/// Utiliza o framework MSTest e Moq para simular interações com a base de dados e testar o comportamento dos métodos do controlador ItensEmprestimoController.
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
using MockQueryable.Moq;


namespace CommuniCareTests
{
    /// <summary>
    /// Classe que contém os métodos de teste para o controlador de itens de empréstimo (ItensEmprestimoController).
    /// Utiliza o framework MSTest para validar o comportamento das funcionalidades de empréstimo.
    /// </summary>

    [TestClass]
    public class ItensEmprestimoTests
    {
        private ItensEmprestimoController _controller;
        private Mock<CommuniCareContext> _mockContext;

        /// <summary>
        /// Método de inicialização do teste. Configura o ambiente antes da execução de cada teste.
        /// Cria uma instância mock do contexto de dados e do controlador, preparando-os para a execução dos testes.
        /// </summary>

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CommuniCareContext>();

            _controller = new ItensEmprestimoController(_mockContext.Object);
        }


        #region Utilizador

        #region AdicionarItemEmprestimo

        /// <summary>
        /// Testa o comportamento do método AdicionarItemEmprestimo quando os dados fornecidos são válidos.
        /// Espera-se que o método retorne um sucesso (status 200) com a mensagem de confirmação de adição do item de empréstimo e a indicação de que ele está aguardando validação.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task AdicionarItemEmprestimo_ValidData_ReturnsOk()
        {

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


            var result = await _controller.AdicionarItemEmprestimo(itemData);


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Item de empréstimo adicionado com sucesso. Aguardando validação.", okResult.Value);
        }


        /// <summary>
        /// Testa o comportamento do método AdicionarItemEmprestimo quando o utilizador não está autenticado.
        /// Espera-se que o método retorne uma resposta não autorizada (status 401) com uma mensagem informando que o utilizador não está autenticado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task AdicionarItemEmprestimo_InvalidData_ReturnsUnauthorized()
        {

            var itemData = new ItemEmprestimoDTO
            {
                NomeItem = "Martelo",
                DescItem = "Martelo de ferro",
                ComissaoCares = 20,
                FotografiaItem = "martelo.jpg"
            };


            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };


            var result = await _controller.AdicionarItemEmprestimo(itemData);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorizedResult.Value);
        }

        #endregion


        #region AdquirirItem

        /// <summary>
        /// Testa o comportamento do método AdquirirItem quando os dados fornecidos são válidos.
        /// Espera-se que o método retorne um sucesso (status 200) com a mensagem confirmando que o pedido de empréstimo foi realizado com sucesso,
        /// aguardando validação do administrador.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task AdquirirItem_ValidData_ReturnsOk()
        {

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


            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };


            var result = await _controller.AdquirirItem(10);


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Pedido de empréstimo efetuado. Aguarde validação do administrador.", okResult.Value);
        }


        /// <summary>
        /// Testa o comportamento do método AdquirirItem quando o utilizador não possui saldo suficiente de Cares para adquirir o item.
        /// Espera-se que o método retorne um erro (status 400) com a mensagem informando que o saldo de Cares é insuficiente.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task AdquirirItem_InvalidData_ReturnsBadRequest_SaldoInsuficiente()
        {

            var utilizador = new Utilizador
            {
                UtilizadorId = 1,
                NomeUtilizador = "Carlos",
                TipoUtilizadorId = 1,
                NumCares = 5
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


            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };


            var result = await _controller.AdquirirItem(10);


            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("Saldo de Cares insuficiente para adquirir este item.", badRequest.Value);
        }


        #endregion


        #region GetItensDisponiveis

        /// <summary>
        /// Testa o comportamento do método GetItensDisponiveis quando o utilizador solicita a lista de itens disponíveis para empréstimo.
        /// Espera-se que o método retorne uma lista de itens que estão disponíveis e associados ao utilizador.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task GetItensDisponiveis_ValidData_ReturnsAvailableItems()
        {

            int userId = 1;

            var relacoes = new List<ItemEmprestimoUtilizador>
            {
                new ItemEmprestimoUtilizador { UtilizadorId = userId, ItemId = 1, TipoRelacao = "Dono" }
            }.AsQueryable();

            var itens = new List<ItemEmprestimo>
            {
             new ItemEmprestimo { ItemId = 2, NomeItem = "Martelo", Disponivel = 1 },
             new ItemEmprestimo { ItemId = 1, NomeItem = "Chave", Disponivel = 1 }
            }.AsQueryable();

            _mockContext.Setup(c => c.ItemEmprestimoUtilizadores).Returns(relacoes.BuildMockDbSet().Object);
            _mockContext.Setup(c => c.ItensEmprestimo).Returns(itens.BuildMockDbSet().Object);


            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };


            var result = await _controller.GetItensDisponiveis();


            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            var itensDisponiveis = okResult.Value as IEnumerable<ItemEmprestimo>;
            Assert.AreEqual(1, itensDisponiveis.Count());
        }


        /// <summary>
        /// Testa o comportamento do método GetItensDisponiveis quando o utilizador não está autenticado.
        /// Espera-se que o método retorne um erro de "Não Autenticado" (Unauthorized - 401), indicando que o utilizador precisa estar autenticado para acessar os itens disponíveis.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task GetItensDisponiveis_InvalidUser_ReturnsUnauthorized()
        {

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal()
                }
            };


            var result = await _controller.GetItensDisponiveis();


            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
            var unauthorized = result.Result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorized.Value);
        }

        #endregion


        #region GetMeusItens

        /// <summary>
        /// Testa o comportamento do método GetMeusItens quando o utilizador está autenticado e tem itens que ele é dono.
        /// Espera-se que o método retorne os itens que o utilizador possui, ou seja, itens que têm a relação de "Dono" com o utilizador.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task GetMeusItens_ValidData_ReturnsOwnedItems()
        {

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


            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };


            var result = await _controller.GetMeusItens();


            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            var meusItens = okResult.Value as IEnumerable<ItemEmprestimo>;
            Assert.AreEqual(1, meusItens.Count());
            Assert.AreEqual(1, meusItens.First().ItemId);
        }


        /// <summary>
        /// Testa o comportamento do método GetMeusItens quando o utilizador não está autenticado.
        /// Espera-se que o método retorne um erro de "Não Autenticado" (Unauthorized - 401), indicando que o utilizador precisa estar autenticado para acessar os seus itens.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task GetMeusItens_InvalidUser_ReturnsUnauthorized()
        {

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal()
                }
            };


            var result = await _controller.GetMeusItens();


            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
            var unauthorized = result.Result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorized.Value);
        }

        #endregion


        #region EliminarItemEmprestimo

        /// <summary>
        /// Testa o comportamento do método EliminarItemEmprestimo quando o utilizador autenticado é o proprietário do item.
        /// Espera-se que o item seja removido corretamente e que o método retorne um status 204 No Content, indicando que a operação foi bem-sucedida, mas não há conteúdo a ser retornado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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


        /// <summary>
        /// Testa o comportamento do método EliminarItemEmprestimo quando o utilizador não está autenticado.
        /// Espera-se que o método retorne um erro de "Não Autenticado" (Unauthorized - 401), indicando que o utilizador precisa estar autenticado para remover um item de empréstimo.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task EliminarItemEmprestimo_Unauthenticated_ReturnsUnauthorized()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal()
                }
            };

            var result = await _controller.EliminarItemEmprestimo(10);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }


        /// Testa o comportamento do método EliminarItemEmprestimo quando o item não é encontrado.
        /// Espera-se que o método retorne um erro de "Item não encontrado" (NotFound - 404), indicando que o item de empréstimo solicitado para remoção não existe no sistema.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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


        /// <summary>
        /// Testa o comportamento do método EliminarItemEmprestimo quando o empréstimo relacionado ao item não é encontrado.
        /// Espera-se que o método retorne um erro de "Empréstimo não encontrado" (NotFound -404), indicando que não existe um empréstimo relacionado ao item de empréstimo no sistema.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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

        /// <summary>
        /// Testa o comportamento do método EliminarItemEmprestimo quando o utilizador não é o proprietário do item.
        /// Espera-se que o método retorne um erro de "Não Autorizado" (Unauthorized - 401), indicando que o utilizador não tem permissão para remover o item de empréstimo.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>
        /// 
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

        #endregion


        #region AtualizarDescricaoItem

        /// <summary>
        /// Testa o comportamento do método AtualizarDescricaoItem quando o utilizador é o proprietário do item.
        /// Espera-se que o método retorne a memsagem HTTP 204, indicando que a descrição do item foi atualizada com sucesso.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>
        /// 
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


        /// <summary>
        /// Testa o comportamento do método AtualizarDescricaoItem quando o utilizador não está autenticado.
        /// Espera-se que o método retorne um erro "Não Autenticado" (Unauthorized), indicando que o utilizador precisa estar autenticado para atualizar a descrição de um item.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task AtualizarDescricaoItem_Unauthenticated_ReturnsUnauthorized()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal()
                }
            };

            var result = await _controller.AtualizarDescricaoItem(10, "Nova descrição");

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.AreEqual("Utilizador não autenticado.", unauthorized.Value);
        }

        /// <summary>
        /// Testa o comportamento do método AtualizarDescricaoItem quando o utilizador tem um ID inválido.
        /// Espera-se que o método retorne um erro "ID de utilizador inválido" (Unauthorized), indicando que o utilizador autenticado não tem um ID válido.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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


        /// <summary>
        /// Testa o comportamento do método AtualizarDescricaoItem quando o item não é encontrado.
        /// Espera-se que o método retorne um erro de "Item não encontrado" (NotFound), indicando que o item de empréstimo solicitado não existe no sistema.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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


        /// <summary>
        /// Testa o comportamento do método AtualizarDescricaoItem quando o item não é encontrado.
        /// Espera-se que o método retorne um erro de "Item não encontrado" (NotFound), indicando que o item de empréstimo solicitado não existe no sistema.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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

        /// <summary>
        /// Testa o comportamento do método AtualizarDescricaoItem quando o utilizador não é o proprietário do item.
        /// Espera-se que o método retorne um erro de "Não Autorizado" (Unauthorized), indicando que o utilizador não tem permissão para atualizar o item de empréstimo.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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

        #endregion


        #endregion

        #region Administrador

        #region ValidarItemEmprestimo

        /// <summary>
        /// Testa o comportamento do método ValidarItemEmprestimo quando o utilizador é um administrador válido.
        /// Espera-se que o método retorne uma resposta "OK", indicando que o item foi validado e disponibilizado com sucesso.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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

        /// <summary>
        /// Testa o comportamento do método ValidarItemEmprestimo quando o utilizador não está autenticado.
        /// Espera-se que o método retorne uma resposta "Unauthorized", indicando que o utilizador precisa estar autenticado para validar um item de empréstimo.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarItemEmprestimo_Unauthorized_ReturnsUnauthorized()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal()
                }
            };

            var result = await _controller.ValidarItemEmprestimo(10);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        /// <summary>
        /// Testa o comportamento do método ValidarItemEmprestimo quando o utilizador não é um administrador.
        /// Espera-se que o método retorne uma resposta "Forbid" (Proibido), indicando que apenas administradores têm permissão para validar um item de empréstimo.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarItemEmprestimo_NotAdmin_ReturnsForbid()
        {
            int userId = 1;
            int itemId = 10;

            var user = new Utilizador { UtilizadorId = userId, TipoUtilizadorId = 1 };

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

        /// <summary>
        /// Testa o comportamento do método ValidarItemEmprestimo quando o item não é encontrado.
        /// Espera-se que o método retorne uma resposta "NotFound" (Não Encontrado), indicando que o item de empréstimo não foi encontrado na base de dados.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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

        /// <summary>
        /// Testa o comportamento do método ValidarItemEmprestimo quando o item já foi validado e está disponível.
        /// Espera-se que o método retorne um erro de "BadRequest" (Requisição Inválida), indicando que o item já foi validado anteriormente.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ValidarItemEmprestimo_ItemAlreadyValidated_ReturnsBadRequest()
        {
            int adminId = 1;
            int itemId = 10;

            var item = new ItemEmprestimo { ItemId = itemId, NomeItem = "Martelo", Disponivel = 1 };
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

        #endregion


        #region RejeitarItemEmprestimo

        /// <summary>
        /// Testa o comportamento do método RejeitarItemEmprestimo quando um administrador válido rejeita um item.
        /// Espera-se que o método retorne um código de resposta "Ok" com a mensagem de sucesso.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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

        /// <summary>
        /// Testa o comportamento do método RejeitarItemEmprestimo quando o utilizador não está autenticado.
        /// Espera-se que o método retorne um erro de "Não Autorizado" (Unauthorized), indicando que o utilizador não tem permissão para rejeitar o item.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task RejeitarItemEmprestimo_Unauthorized_ReturnsUnauthorized()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal()
                }
            };

            var result = await _controller.RejeitarItemEmprestimo(10);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }


        /// <summary>
        /// Testa o comportamento do método RejeitarItemEmprestimo quando o utilizador não é administrador.
        /// Espera-se que o método retorne um código de resposta "Forbid" (proibido), indicando que apenas administradores têm permissão para rejeitar itens.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task RejeitarItemEmprestimo_NotAdmin_ReturnsForbid()
        {
            int userId = 1;
            var user = new Utilizador { UtilizadorId = userId, TipoUtilizadorId = 1 };

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

        /// <summary>
        /// Testa o comportamento do método RejeitarItemEmprestimo quando o item não é encontrado.
        /// Espera-se que o método retorne um erro "Item não encontrado", indicando que o item que se tenta rejeitar não existe na base de dados.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task RejeitarItemEmprestimo_ItemNotFound_ReturnsNotFound()
        {
            int adminId = 1;
            var admin = new Utilizador { UtilizadorId = adminId, TipoUtilizadorId = 2 };

            _mockContext.Setup(c => c.Utilizadores.FindAsync(adminId)).ReturnsAsync(admin);
            _mockContext.Setup(c => c.ItensEmprestimo)
                .Returns(new List<ItemEmprestimo>().AsQueryable().BuildMockDbSet().Object);

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

        /// <summary>
        /// Testa o comportamento do método RejeitarItemEmprestimo quando o item não tem um "dono" atribuído.
        /// Espera-se que o método retorne sucesso (OK), indicando que o item foi rejeitado e removido com sucesso, mesmo que não tenha um dono associado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

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
                .Returns(new List<ItemEmprestimoUtilizador>().AsQueryable().BuildMockDbSet().Object);
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

        #endregion

        #endregion

    }
}