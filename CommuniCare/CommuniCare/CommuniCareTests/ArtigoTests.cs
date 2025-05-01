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


namespace CommuniCareTests
{
    [TestClass]
    public class ArtigosTests
    {
        private ArtigosController _controller;
        private Mock<DbSet<Artigo>> _mockArtigosDbSet;
        private Mock<DbSet<Loja>> _mockLojasDbSet;
        private Mock<DbSet<Utilizador>> _mockUtilizadoresDbSet;
        private Mock<CommuniCareContext> _mockContext;

        [TestInitialize]
        public void Setup()
        {
            _mockArtigosDbSet = new Mock<DbSet<Artigo>>();
            _mockLojasDbSet = new Mock<DbSet<Loja>>();
            _mockUtilizadoresDbSet = new Mock<DbSet<Utilizador>>();

            _mockContext = new Mock<CommuniCareContext>();

            _mockContext.Setup(m => m.Artigos).Returns(_mockArtigosDbSet.Object);
            _mockContext.Setup(m => m.Lojas).Returns(_mockLojasDbSet.Object);
            _mockContext.Setup(m => m.Utilizadores).Returns(_mockUtilizadoresDbSet.Object);

            _controller = new ArtigosController(_mockContext.Object);
        }

        #region Testes de Sucesso

        #region GetArtigosDisponiveis

        [TestMethod]
        public async Task GetArtigosDisponiveis_ReturnsOk_WhenArtigosExist()
        {
            // Arrange
            var artigosList = new List<Artigo>
            {
                new Artigo { ArtigoId = 1, NomeArtigo = "Artigo 1", Estado = EstadoArtigo.Disponivel },
                new Artigo { ArtigoId = 2, NomeArtigo = "Artigo 2", Estado = EstadoArtigo.Disponivel }
            };

            var mockArtigosDbSet = artigosList.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.Artigos).Returns(mockArtigosDbSet.Object);

            // Act
            var result = await _controller.GetArtigosDisponiveis();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(2, ((List<Artigo>)okResult.Value).Count);
        }

        #endregion

        #endregion

         #region Testes de Erro

        #region GetArtigosDisponiveis

        [TestMethod]
        public async Task GetArtigosDisponiveis_ReturnsOk_WhenNoArtigosExist()
        {
            // Arrange
            var artigosList = new List<Artigo>(); // Lista vazia

            var mockArtigosDbSet = artigosList.AsQueryable().BuildMockDbSet();
            _mockContext.Setup(c => c.Artigos).Returns(mockArtigosDbSet.Object);

            // Act
            var result = await _controller.GetArtigosDisponiveis();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(0, ((List<Artigo>)okResult.Value).Count);
        }

        #endregion

        #endregion

    }
}