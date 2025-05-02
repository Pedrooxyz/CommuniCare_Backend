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
using CommuniCare; // Adicionado para usar o IConfiguration

namespace CommuniCareTests
{
    [TestClass]
    public class UtilizadorControllerTests
    {
        private UtilizadoresController _controller;
        private Mock<DbSet<Utilizador>> _mockUtilizadoresDbSet;
        private Mock<CommuniCareContext> _mockContext;
        private Mock<IConfiguration> _mockConfiguration; // Mock da IConfiguration

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

        [TestMethod]
        public async Task GetCurrentUser_ValidUser_ReturnsUtilizadorInfoDto()
        {
            // Arrange
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

            // Mock usuário autenticado com o mesmo ID
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

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
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

        [TestMethod]
        public async Task GetCurrentUser_MissingClaim_ReturnsUnauthorized()
        {
            // Arrange
            var userClaims = new List<Claim>(); // Sem o NameIdentifier
            var userIdentity = new ClaimsIdentity(userClaims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task GetCurrentUser_UserNotFoundInDatabase_ReturnsUnauthorized()
        {
            // Arrange
            var userId = 99; // ID que não existe no mock
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

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Register_ModelStateInvalid_ReturnsBadRequest()
        {
            // Arrange
            var dto = new UtilizadorDTO(); // Dados incompletos
            _controller.ModelState.AddModelError("Email", "Obrigatório");

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Register_EmailAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
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

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("Já existe uma conta com este email.", badRequest.Value);
        }

        [TestMethod]
        public async Task Register_ValidData_ReturnsOkWithToken()
        {
            // Arrange
            var dto = new UtilizadorDTO
            {
                NomeUtilizador = "Joana",
                Password = "123456",
                Email = "joana@example.com"
            };

            var contactos = new List<Contacto>().AsQueryable();
            var mockContactos = contactos.BuildMockDbSet();
            _mockContext.Setup(c => c.Contactos).Returns(mockContactos.Object);

            var cps = new List<Cp> { new Cp { CPostal = "0000-000", Localidade = "000000" } }.AsQueryable();
            var mockCps = cps.BuildMockDbSet();
            _mockContext.Setup(c => c.Cps).Returns(mockCps.Object);

            var moradas = new List<Morada>().AsQueryable();
            var mockMoradas = moradas.BuildMockDbSet();
            _mockContext.Setup(c => c.Morada).Returns(mockMoradas.Object);

            var utilizadores = new List<Utilizador>
    {
        new Utilizador { UtilizadorId = 99, TipoUtilizadorId = 2, EstadoUtilizador = EstadoUtilizador.Ativo }
    }.AsQueryable();
            var mockUtilizadores = utilizadores.BuildMockDbSet();
            _mockContext.Setup(c => c.Utilizadores).Returns(mockUtilizadores.Object);

            var notificacoes = new List<Notificacao>().AsQueryable();
            var mockNotificacoes = notificacoes.BuildMockDbSet();
            _mockContext.Setup(c => c.Notificacaos).Returns(mockNotificacoes.Object);

            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            dynamic value = okResult.Value;

            var message = value.GetType().GetProperty("Message")?.GetValue(value)?.ToString();
            Assert.AreEqual("Conta criada com sucesso! Aguardando aprovação de um administrador.", message);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task UpdateFotoUrl_ValidRequest_UpdatesAndReturnsNoContent()
        {
            // Arrange
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

            // Act
            var result = await _controller.UpdateFotoUrl(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            Assert.AreEqual("new_photo.jpg", utilizador.FotoUtil);
        }


        [TestMethod]
        public async Task UpdateFotoUrl_EmptyFotoUrl_ReturnsBadRequest()
        {
            // Arrange
            var dto = new FotoUrlDto
            {
                FotoUrl = "   " // Empty / whitespace
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

            // Act
            var result = await _controller.UpdateFotoUrl(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("FotoUrl cannot be empty.", badRequest.Value);
        }


        [TestMethod]
        public async Task UpdateFotoUrl_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var userId = 99;
            var utilizadores = new List<Utilizador>().AsQueryable(); // Vazio
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

            // Act
            var result = await _controller.UpdateFotoUrl(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }


        [TestMethod]
        public async Task UpdateFotoUrl_InvalidUserIdClaim_ReturnsUnauthorized()
        {
            // Arrange
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

            // Act
            var result = await _controller.UpdateFotoUrl(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ////
        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
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

            // Use real configuration instead of mocking Get<JwtSettings>()
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

            // Act
            var result = await _controller.Login(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var value = okResult.Value;
            var token = value.GetType().GetProperty("Token")?.GetValue(value)?.ToString();
            var message = value.GetType().GetProperty("Message")?.GetValue(value)?.ToString();

            Assert.IsNotNull(token);
            Assert.AreEqual("Login efetuado com sucesso", message);
        }

        [TestMethod]
        public async Task Login_InvalidEmailOrPassword_ReturnsUnauthorized()
        {
            // Arrange
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

            // Act
            var result = await _controller.Login(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Email ou password inválidos.", unauthorizedResult.Value);
        }

        [TestMethod]
        public async Task Login_InactiveUser_ReturnsUnauthorized()
        {
            // Arrange
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

            // Act
            var result = await _controller.Login(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("A sua conta ainda não está ativa.", unauthorizedResult.Value);
        }

        [TestMethod]
        public async Task Login_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Email inválido");

            var dto = new LoginDTO
            {
                Email = "invalid email",
                Password = "validPassword"
            };

            // Act
            var result = await _controller.Login(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new LoginDTO
            {
                Email = "user@example.com",
                Password = "wrongPassword" // Wrong password
            };

            var contacto = new Contacto
            {
                NumContacto = "user@example.com",
                TipoContactoId = 1,
                Utilizador = new Utilizador
                {
                    UtilizadorId = 1,
                    Password = BCrypt.Net.BCrypt.HashPassword("correctPassword"), // Real password
                    EstadoUtilizador = EstadoUtilizador.Ativo,
                    TipoUtilizadorId = 1
                }
            };

            var contactos = new List<Contacto> { contacto }.AsQueryable();
            var mockContactoDbSet = contactos.BuildMockDbSet();

            _mockContext.Setup(c => c.Contactos).Returns(mockContactoDbSet.Object);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Email ou password inválidos.", unauthorizedResult.Value);
        }


        [TestMethod]
        public async Task ObterSaldo_ValidRequest_ReturnsSaldo()
        {
            // Arrange
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

            // Act
            var result = await _controller.ObterSaldo();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var saldo = (int)okResult.Value.GetType().GetProperty("Saldo")?.GetValue(okResult.Value);

            Assert.AreEqual(100, saldo);
        }

        [TestMethod]
        public async Task ObterSaldo_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var utilizadorId = 1;
            var utilizadores = new List<Utilizador>().AsQueryable(); // Usuário não encontrado
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

            // Act
            var result = await _controller.ObterSaldo();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Utilizador não encontrado.", notFoundResult.Value);
        }

       

        [TestMethod]
        public async Task ApagarConta_InvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
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

            // Act
            var result = await _controller.ApagarConta(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Password incorreta.", unauthorizedResult.Value);
        }


    }
}