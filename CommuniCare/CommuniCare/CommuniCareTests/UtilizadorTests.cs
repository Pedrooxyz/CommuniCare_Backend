/// <summary>
/// Classe de testes unitários para o controlador UtilizadoresController.
/// Este conjunto de testes visa validar as funcionalidades implementadas no controlador de utilizadores.
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
using Microsoft.Extensions.Configuration;
using CommuniCare;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;


namespace CommuniCareTests
{
    /// <summary>
    /// Classe de testes unitários para o controlador de utilizadores.
    /// Contém métodos que validam o comportamento de diferentes funcionalidades do controlador.
    /// </summary>

    [TestClass]
    public class UtilizadorControllerTests
    {
        private UtilizadoresController _controller;
        private Mock<DbSet<Utilizador>> _mockUtilizadoresDbSet;
        private Mock<CommuniCareContext> _mockContext;
        private Mock<IConfiguration> _mockConfiguration;

        /// <summary>
        /// Método de inicialização que prepara o ambiente de testes antes de cada execução de teste.
        /// Configura os mocks necessários, como o contexto de dados e a configuração, 
        /// além de instanciar o controlador de utilizadores.
        /// </summary>

        [TestInitialize]
        public void Setup()
        {
            _mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();
            _mockContext = new Mock<CommuniCareContext>();
            _mockConfiguration = new Mock<IConfiguration>();


            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("chave-secreta-de-teste");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("issuer-teste");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("audience-teste");

            _mockContext.Setup(c => c.Utilizadores).Returns(_mockUtilizadoresDbSet.Object);

            _controller = new UtilizadoresController(_mockContext.Object, _mockConfiguration.Object);
        }

        #region GetCurrentUser

        /// <summary>
        /// Testa o comportamento do método GetCurrentUser quando um usuário válido está autenticado.
        /// Espera-se que o método retorne um objeto UtilizadorInfoDto com as informações do usuário.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task GetCurrentUser_ValidUser_ReturnsUtilizadorInfoDto()
        {

            var userId = 1;
            var utilizador = new Utilizador
            {
                UtilizadorId = userId,
                NomeUtilizador = "João",
                FotoUtil = "foto.jpg",
                NumCares = 3,
                MoradaId = 100,
                TipoUtilizadorId = 2
            };

            var utilizadores = new List<Utilizador> { utilizador }.AsQueryable();
            var mockDbSet = utilizadores.BuildMockDbSet();

            _mockContext.Setup(c => c.Utilizadores).Returns(mockDbSet.Object);


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

            var result = await _controller.GetCurrentUser();


            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            var dto = okResult.Value as UtilizadorInfoDto;

            Assert.IsNotNull(dto);
            Assert.AreEqual(utilizador.UtilizadorId, dto.UtilizadorId);
            Assert.AreEqual(utilizador.NomeUtilizador, dto.NomeUtilizador);
            Assert.AreEqual(utilizador.FotoUtil, dto.FotoUtil);
            Assert.AreEqual(utilizador.NumCares, dto.NumCares);
            Assert.AreEqual(utilizador.MoradaId, dto.MoradaId);
            Assert.AreEqual(utilizador.TipoUtilizadorId, dto.TipoUtilizadorId);
        }


        /// <summary>
        /// Testa o comportamento do método GetCurrentUser quando o usuário não possui o claim necessário.
        /// Espera-se que o método retorne um resultado Unauthorized, indicando que o usuário não está autenticado corretamente.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task GetCurrentUser_MissingClaim_ReturnsUnauthorized()
        {

            var userClaims = new List<Claim>();
            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };


            var result = await _controller.GetCurrentUser();


            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedResult));
        }

        /// <summary>
        /// Testa o comportamento do método GetCurrentUser quando o usuário não é encontrado no banco de dados.
        /// Espera-se que o método retorne um resultado Unauthorized, indicando que o usuário não existe na base de dados.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task GetCurrentUser_UserNotFoundInDatabase_ReturnsUnauthorized()
        {

            var userId = 99;
            var utilizadores = new List<Utilizador>().AsQueryable();
            var mockDbSet = utilizadores.BuildMockDbSet();

            _mockContext.Setup(c => c.Utilizadores).Returns(mockDbSet.Object);

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


            var result = await _controller.GetCurrentUser();


            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedResult));
        }

        #endregion


        #region Register

        //// <summary>
        /// Testa o comportamento do método Register quando o ModelState é inválido.
        /// Espera-se que o método retorne um resultado BadRequest, indicando que houve erros de validação no modelo.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task Register_ModelStateInvalid_ReturnsBadRequest()
        {

            var dto = new UtilizadorDTO();
            _controller.ModelState.AddModelError("Email", "Obrigatório");


            var result = await _controller.Register(dto);


            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }


        /// <summary>
        /// Testa o comportamento do método Register quando o email fornecido já está registrado.
        /// Espera-se que o método retorne um resultado BadRequest, indicando que já existe uma conta associada ao email fornecido.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>
        [TestMethod]
        public async Task Register_EmailAlreadyExists_ReturnsBadRequest()
        {

            var dto = new UtilizadorDTO
            {
                NomeUtilizador = "João",
                Password = "123456",
                Email = "joao@example.com"
            };

            var contactos = new List<Contacto>
            {
                new Contacto { NumContacto = "joao@example.com" }
            }.AsQueryable();

            var mockContactos = contactos.BuildMockDbSet();
            _mockContext.Setup(c => c.Contactos).Returns(mockContactos.Object);


            var result = await _controller.Register(dto);


            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("Já existe uma conta com este email.", badRequest.Value);
        }


        /// <summary>
        /// Testa o comportamento do método Register quando os dados fornecidos são válidos.
        /// Espera-se que o método retorne um resultado Ok, indicando que a conta foi criada com sucesso, e que uma mensagem de sucesso seja retornada, informando que a conta está aguardando aprovação de um administrador.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

       

        #endregion


        #region UpdateFotoUrl

        /// <summary>
        /// Testa o comportamento do método UpdateFotoUrl quando uma solicitação válida é feita para atualizar a foto do usuário.
        /// Espera-se que o método atualize corretamente a foto do usuário e retorne um resultado NoContent, indicando que a operação foi realizada com sucesso, sem a necessidade de um conteúdo adicional na resposta.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task UpdateFotoUrl_ValidRequest_UpdatesAndReturnsNoContent()
        {

            var userId = 1;
            var utilizador = new Utilizador
            {
                UtilizadorId = userId,
                NomeUtilizador = "João",
                FotoUtil = "old_photo.jpg"
            };

            var utilizadores = new List<Utilizador> { utilizador }.AsQueryable();
            var mockDbSet = utilizadores.BuildMockDbSet();

            _mockContext.Setup(c => c.Utilizadores).Returns(mockDbSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

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

            var dto = new FotoUrlDto
            {
                FotoUrl = "new_photo.jpg"
            };


            var result = await _controller.UpdateFotoUrl(dto);


            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            Assert.AreEqual("new_photo.jpg", utilizador.FotoUtil);
        }

        /// <summary>
        /// Testa o comportamento do método UpdateFotoUrl quando a URL da foto fornecida está vazia ou contém apenas espaços em branco.
        /// Espera-se que o método retorne um resultado BadRequest com uma mensagem indicando que a URL da foto não pode ser vazia.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task UpdateFotoUrl_EmptyFotoUrl_ReturnsBadRequest()
        {

            var dto = new FotoUrlDto
            {
                FotoUrl = "   "
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


            var result = await _controller.UpdateFotoUrl(dto);


            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("FotoUrl cannot be empty.", badRequest.Value);
        }

        /// <summary>
        /// Testa o comportamento do método UpdateFotoUrl quando o usuário não é encontrado na base de dados.
        /// Espera-se que o método retorne um resultado Unauthorized indicando que o usuário não tem permissão para realizar a operação.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task UpdateFotoUrl_UserNotFound_ReturnsUnauthorized()
        {

            var userId = 99;
            var utilizadores = new List<Utilizador>().AsQueryable();
            var mockDbSet = utilizadores.BuildMockDbSet();

            _mockContext.Setup(c => c.Utilizadores).Returns(mockDbSet.Object);

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

            var dto = new FotoUrlDto
            {
                FotoUrl = "new_photo.jpg"
            };


            var result = await _controller.UpdateFotoUrl(dto);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        /// <summary>
        /// Testa o comportamento do método UpdateFotoUrl quando o identificador de utilizador (UserId) fornecido nos claims é inválido.
        /// Espera-se que o método retorne um resultado Unauthorized, indicando que o utilizador não tem permissão para realizar a operação devido a um ID inválido.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task UpdateFotoUrl_InvalidUserIdClaim_ReturnsUnauthorized()
        {

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "invalid_id")
            };

            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            var dto = new FotoUrlDto
            {
                FotoUrl = "photo.jpg"
            };


            var result = await _controller.UpdateFotoUrl(dto);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        #endregion


        #region Login

        /// <summary>
        /// Testa o comportamento do método Login quando as credenciais fornecidas são válidas.
        /// Espera-se que o método retorne um resultado Ok, com um token JWT e uma mensagem de sucesso indicando que o login foi efetuado com sucesso.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {

            var dto = new LoginDTO
            {
                Email = "user@example.com",
                Password = "validPassword"
            };

            var utilizador = new Utilizador
            {
                UtilizadorId = 1,
                Password = BCrypt.Net.BCrypt.HashPassword("validPassword"),
                EstadoUtilizador = EstadoUtilizador.Ativo,
                TipoUtilizadorId = 1
            };

            var contacto = new Contacto
            {
                NumContacto = "user@example.com",
                TipoContactoId = 1,
                Utilizador = utilizador
            };

            var contactos = new List<Contacto> { contacto }.AsQueryable();
            var mockContactoDbSet = contactos.BuildMockDbSet();

            _mockContext.Setup(c => c.Contactos).Returns(mockContactoDbSet.Object);


            var inMemorySettings = new Dictionary<string, string>
            {
                {"JwtSettings:SecretKey", "super_secret_key_for_testing_purposes_1234567890"},
                {"JwtSettings:Issuer", "test_issuer"},
                {"JwtSettings:Audience", "test_audience"},
                {"JwtSettings:ExpirationMinutes", "60"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _controller = new UtilizadoresController(_mockContext.Object, configuration);


            var result = await _controller.Login(dto);


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var value = okResult.Value;
            var token = value.GetType().GetProperty("Token")?.GetValue(value)?.ToString();
            var message = value.GetType().GetProperty("Message")?.GetValue(value)?.ToString();

            Assert.IsNotNull(token);
            Assert.AreEqual("Login efetuado com sucesso", message);
        }

        /// <summary>
        /// Testa o comportamento do método Login quando o email ou a password fornecidos são inválidos.
        /// Espera-se que o método retorne um resultado Unauthorized, com uma mensagem indicando que o email ou a password estão incorretos.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task Login_InvalidEmailOrPassword_ReturnsUnauthorized()
        {

            var dto = new LoginDTO
            {
                Email = "invalid@example.com",
                Password = "wrongPassword"
            };

            var contacto = new Contacto
            {
                NumContacto = "user@example.com",
                TipoContactoId = 1,
                Utilizador = new Utilizador
                {
                    UtilizadorId = 1,
                    Password = BCrypt.Net.BCrypt.HashPassword("validPassword"),
                    EstadoUtilizador = EstadoUtilizador.Ativo,
                    TipoUtilizadorId = 1
                }
            };

            var contactos = new List<Contacto> { contacto }.AsQueryable();
            var mockContactoDbSet = contactos.BuildMockDbSet();

            _mockContext.Setup(c => c.Contactos).Returns(mockContactoDbSet.Object);


            var result = await _controller.Login(dto);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Email ou password inválidos.", unauthorizedResult.Value);
        }

        /// <summary>
        /// Testa o comportamento do método Login quando o utilizador está inativo.
        /// Espera-se que o método retorne um resultado Unauthorized, com uma mensagem indicando que a conta do utilizador ainda não está ativa.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task Login_InactiveUser_ReturnsUnauthorized()
        {

            var dto = new LoginDTO
            {
                Email = "user@example.com",
                Password = "validPassword"
            };

            var utilizador = new Utilizador
            {
                UtilizadorId = 1,
                Password = BCrypt.Net.BCrypt.HashPassword("validPassword"),
                EstadoUtilizador = EstadoUtilizador.Inativo,
                TipoUtilizadorId = 1
            };

            var contacto = new Contacto
            {
                NumContacto = "user@example.com",
                TipoContactoId = 1,
                Utilizador = utilizador
            };

            var contactos = new List<Contacto> { contacto }.AsQueryable();
            var mockContactoDbSet = contactos.BuildMockDbSet();

            _mockContext.Setup(c => c.Contactos).Returns(mockContactoDbSet.Object);


            var result = await _controller.Login(dto);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("A sua conta ainda não está ativa.", unauthorizedResult.Value);
        }

        /// <summary>
        /// Testa o comportamento do método Login quando o estado do modelo é inválido.
        /// Espera-se que o método retorne um resultado BadRequest, indicando que o modelo fornecido não é válido.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task Login_InvalidModelState_ReturnsBadRequest()
        {

            _controller.ModelState.AddModelError("Email", "Email inválido");

            var dto = new LoginDTO
            {
                Email = "invalid email",
                Password = "validPassword"
            };


            var result = await _controller.Login(dto);


            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Testa o comportamento do método Login quando o token é inválido (neste caso, a password está incorreta).
        /// Espera-se que o método retorne um resultado Unauthorized, indicando que o login falhou devido a credenciais inválidas.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task Login_InvalidToken_ReturnsUnauthorized()
        {

            var dto = new LoginDTO
            {
                Email = "user@example.com",
                Password = "wrongPassword"
            };

            var contacto = new Contacto
            {
                NumContacto = "user@example.com",
                TipoContactoId = 1,
                Utilizador = new Utilizador
                {
                    UtilizadorId = 1,
                    Password = BCrypt.Net.BCrypt.HashPassword("correctPassword"),
                    EstadoUtilizador = EstadoUtilizador.Ativo,
                    TipoUtilizadorId = 1
                }
            };

            var contactos = new List<Contacto> { contacto }.AsQueryable();
            var mockContactoDbSet = contactos.BuildMockDbSet();

            _mockContext.Setup(c => c.Contactos).Returns(mockContactoDbSet.Object);


            var result = await _controller.Login(dto);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Email ou password inválidos.", unauthorizedResult.Value);
        }

        #endregion


        #region ObterSaldo

        /// <summary>
        /// Testa o comportamento do método ObterSaldo quando a requisição é válida.
        /// Espera-se que o método retorne um saldo associado ao utilizador, com base no ID do utilizador passado no pedido.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ObterSaldo_ValidRequest_ReturnsSaldo()
        {

            var utilizadorId = 1;
            var utilizador = new Utilizador
            {
                UtilizadorId = utilizadorId,
                NumCares = 100
            };

            var utilizadores = new List<Utilizador> { utilizador }.AsQueryable();
            var mockDbSetUtilizadores = utilizadores.BuildMockDbSet();

            _mockContext.Setup(c => c.Utilizadores).Returns(mockDbSetUtilizadores.Object);

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString())
            };

            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };


            var result = await _controller.ObterSaldo();


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var saldo = (int)okResult.Value.GetType().GetProperty("Saldo")?.GetValue(okResult.Value);

            Assert.AreEqual(100, saldo);
        }

        /// <summary>
        /// Testa o comportamento do método ObterSaldo quando o utilizador não é encontrado.
        /// Espera-se que o método retorne um erro NotFound com uma mensagem indicando que o utilizador não foi encontrado.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ObterSaldo_UserNotFound_ReturnsNotFound()
        {

            var utilizadorId = 1;
            var utilizadores = new List<Utilizador>().AsQueryable();
            var mockDbSetUtilizadores = utilizadores.BuildMockDbSet();

            _mockContext.Setup(c => c.Utilizadores).Returns(mockDbSetUtilizadores.Object);

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString())
            };

            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };


            var result = await _controller.ObterSaldo();


            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Utilizador não encontrado.", notFoundResult.Value);
        }

        #endregion


        #region ApagarConta

        /// <summary>
        /// Testa o comportamento do método ApagarConta quando a password fornecida está incorreta.
        /// Espera-se que o método retorne um erro Unauthorized com uma mensagem indicando que a password está incorreta.
        /// </summary>
        /// <returns>Uma tarefa que representa a execução do teste unitário.</returns>

        [TestMethod]
        public async Task ApagarConta_InvalidPassword_ReturnsUnauthorized()
        {

            var utilizadorId = 1;
            var utilizador = new Utilizador
            {
                UtilizadorId = utilizadorId,
                Password = BCrypt.Net.BCrypt.HashPassword("validPassword"),
                EstadoUtilizador = EstadoUtilizador.Ativo
            };

            var utilizadores = new List<Utilizador> { utilizador }.AsQueryable();
            var mockDbSetUtilizadores = utilizadores.BuildMockDbSet();

            _mockContext.Setup(c => c.Utilizadores).Returns(mockDbSetUtilizadores.Object);

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString())
            };

            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            var dto = new ConfirmarPasswordDTO { Password = "wrongPassword" };


            var result = await _controller.ApagarConta(dto);


            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Password incorreta.", unauthorizedResult.Value);
        }
        #endregion

    }
}