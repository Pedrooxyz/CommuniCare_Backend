/// <summary>
/// Namespace que contém os testes unitários da aplicação CommuniCare.
/// Utiliza o framework MSTest e Moq para simular interações com a base de dados e testar o comportamento dos métodos do controlador VendaController.
/// </summary>
/// 

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
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MockQueryable.EntityFrameworkCore;
using CommuniCare;


namespace CommuniCareTests
{
    /// <summary>
    /// Classe de testes unitários para o controlador VendaController.
    /// Testa as funcionalidades do controlador relacionadas à compra e transações de vendas.
    /// </summary>

    [TestClass]
    public class VendaTests
    {
        private VendasController _controller;
        private Mock<DbSet<Venda>> _mockVendasDbSet;
        private Mock<DbSet<Transacao>> _mockTransacoesDbSet;
        private Mock<DbSet<Utilizador>> _mockUtilizadoresDbSet;
        private Mock<DbSet<Loja>> _mockLojasDbSet;
        private Mock<CommuniCareContext> _mockContext;
        private Mock<EmailService> _mockEmailService;
        private Mock<TransacaoServico> _mockTransacaoServico;

        /// <summary>
        /// Configura o ambiente para os testes, inicializando os mocks e o controlador.
        /// Este método é chamado antes de cada execução de teste para garantir um estado limpo para os testes.
        /// </summary>

        [TestInitialize]
        public void Setup()
        {

            _mockVendasDbSet = new Mock<DbSet<Venda>>();
            _mockTransacoesDbSet = new Mock<DbSet<Transacao>>();
            _mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();
            _mockLojasDbSet = new Mock<DbSet<Loja>>();

            _mockContext = new Mock<CommuniCareContext>();

            _mockTransacaoServico = new Mock<TransacaoServico>(_mockContext.Object);



            _mockContext = new Mock<CommuniCareContext>();


            _mockContext.Setup(m => m.Venda).Returns(_mockVendasDbSet.Object);
            _mockContext.Setup(m => m.Transacoes).Returns(_mockTransacoesDbSet.Object);
            _mockContext.Setup(m => m.Utilizadores).Returns(_mockUtilizadoresDbSet.Object);
            _mockContext.Setup(m => m.Lojas).Returns(_mockLojasDbSet.Object);


            _mockEmailService = new Mock<EmailService>();


            _mockTransacaoServico = new Mock<TransacaoServico>(_mockContext.Object);


            _controller = new VendasController(_mockContext.Object, _mockEmailService.Object);
        }

        #region Testes de Sucesso

        #region Comprar

        /// <summary>
        /// Testa o comportamento do método Comprar quando a compra é realizada com sucesso.
        /// Espera-se que o método retorne um resultado Ok, indicando que a compra foi bem-sucedida, e que a resposta contenha a indicação de sucesso.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        /*[TestMethod]
        public async Task Comprar_ReturnsOk_WhenCompraBemSucedida()
        {
            
            var userId = 1;
            var artigosIds = new List<int> { 10, 20 };
            var pedido = new PedidoCompraDTO { ArtigosIds = artigosIds };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            
            var venda = new Venda();
            var transacao = new Transacao();
            var artigos = new List<Artigo> { new Artigo(), new Artigo() }; 
            var dataCompra = DateTime.Now;

            
            _mockTransacaoServico.Setup(s => s.ProcessarCompraAsync(userId, artigosIds))
                                 .ReturnsAsync((venda, transacao, artigos, dataCompra));

            
            var result = await _controller.Comprar(pedido);

            
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            dynamic resposta = okResult.Value;
            Assert.IsTrue(resposta.Sucesso);
        }*/

        #endregion

        #region ComprarEmail

        /// <summary>
        /// Testa o comportamento do método ComprarEmail quando a compra é realizada com sucesso e o e-mail de comprovação é enviado corretamente.
        /// Espera-se que o método retorne um resultado Ok, indicando que a compra foi bem-sucedida e que o e-mail de confirmação foi enviado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        /*[TestMethod]
        public async Task ComprarEmail_ReturnsOk_WhenCompraBemSucedida()
        {
            
            var userId = 1;
            var artigosIds = new List<int> { 10, 20 };
            var pedido = new PedidoCompraDTO { ArtigosIds = artigosIds };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            //_mockTransacaoServico.Setup(s => s.ProcessarCompraAsync(userId, artigosIds))
            //                     .Returns(Task.CompletedTask);

            var emailMock = new Mock<EmailService>();
            emailMock.Setup(e => e.EnviarComprovativoCompra(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                     .Returns(Task.CompletedTask);

            
            var result = await _controller.ComprarEmail(pedido);

            
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            dynamic resposta = okResult.Value;
            Assert.IsTrue(resposta.Sucesso);
        }*/

        #endregion

        #endregion

        #region Testes de Erro

        #region Comprar

        /// <summary>
        /// Testa o comportamento do método Comprar quando ocorre uma exceção durante o processamento da compra.
        /// Espera-se que o método retorne um resultado BadRequest com uma mensagem de erro apropriada.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        /* [TestMethod]
         public async Task Comprar_ReturnsBadRequest_WhenExceptionOccurs()
         {
             
             var userId = 1;
             var artigosIds = new List<int> { 10, 20 };
             var pedido = new PedidoCompraDTO { ArtigosIds = artigosIds };

             var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
             {
                 new Claim(ClaimTypes.NameIdentifier, userId.ToString())
             }, "mock"));

             _controller.ControllerContext = new ControllerContext
             {
                 HttpContext = new DefaultHttpContext { User = user }
             };

             _mockTransacaoServico
                 .Setup(s => s.ProcessarCompraAsync(userId, artigosIds))
                 .Throws(new System.Exception("Erro ao processar compra"));

             
             var result = await _controller.Comprar(pedido);

             
             Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
             var badRequestResult = result as BadRequestObjectResult;
             var response = badRequestResult.Value;

             var sucesso = response.GetType().GetProperty("Sucesso")?.GetValue(response);
             var erro = response.GetType().GetProperty("Erro")?.GetValue(response)?.ToString();

             Assert.AreEqual(false, sucesso);
             Assert.AreEqual("Erro ao processar compra", erro);
         }*/




        #endregion

        #region CompraEmail

        /// <summary>
        /// Testa o comportamento do método ComprarEmail quando ocorre uma exceção durante o processamento da compra.
        /// Espera-se que o método retorne um resultado BadRequest com uma mensagem de erro apropriada.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        /*[TestMethod]
        public async Task ComprarEmail_ReturnsBadRequest_WhenExceptionOccurs()
        {
            
            var userId = 1;
            var artigosIds = new List<int> { 10, 20 };
            var pedido = new PedidoCompraDTO { ArtigosIds = artigosIds };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            _mockTransacaoServico
                .Setup(s => s.ProcessarCompraAsync(userId, artigosIds))
                .ThrowsAsync(new Exception("Erro ao processar compra"));

           
            var result = await _controller.ComprarEmail(pedido);

            
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            dynamic resposta = badRequestResult.Value;
            Assert.IsFalse(resposta.Sucesso);
            Assert.AreEqual("Erro ao processar compra", resposta.Erro);
        }*/

        #endregion

        #endregion

    }
}