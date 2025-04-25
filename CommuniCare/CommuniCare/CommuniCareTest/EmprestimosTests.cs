using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;
using CommuniCare;
using System.Net;
using System.Collections.Generic;
using CommuniCare.Models;
using System.Net.Http.Json; // Para usar ReadFromJsonAsync
using CommuniCare.DTOs; // Ajuste o namespace conforme a localização de NotificacaoDto
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using System;

namespace CommuniCareTest
{
    [TestClass]
    public class EmprestimosControllerTests
    {
        private readonly HttpClient _httpClient;

        public EmprestimosControllerTests()
        {
            // Cria a instância da aplicação da Web para os testes
            var webAppFactory = new WebApplicationFactory<Program>();
            _httpClient = webAppFactory.CreateDefaultClient(); // Cria o HttpClient
        }

        #region Testes de Sucesso

        //[TestMethod]
        //public async Task ConcluirEmprestimo_DeveConcluirEmprestimoComSucesso()
        //{
        //    // Gera o token de administrador para autenticação
        //    string tokenDeAdministrador = Helpers.GerarTokenAdministrador();
        //    _httpClient.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", tokenDeAdministrador);

        //    // Criação de um empréstimo para teste
        //    var emprestimoParaCriar = new Emprestimo
        //    {
        //        DataIni = DateTime.UtcNow,
        //        Items = new List<ItemEmprestimo>
        //        {
        //            new ItemEmprestimo
        //            {
        //                ItemId = 1,
        //                NomeItem = "Livro de Teste"
        //            }
        //        }
        //    };

        //    // Serializa o empréstimo e envia um POST para criar
        //    var content = JsonContent.Create(emprestimoParaCriar);
        //    var criarResponse = await _httpClient.PostAsync("api/Emprestimos", content);
        //    criarResponse.EnsureSuccessStatusCode();

        //    // Lê o empréstimo criado da resposta
        //    var emprestimoCriado = await criarResponse.Content.ReadFromJsonAsync<Emprestimo>();
        //    Assert.IsNotNull(emprestimoCriado);

        //    // Envia o pedido de conclusão do empréstimo
        //    var response = await _httpClient.PostAsync($"api/Emprestimos/devolucao-item/{emprestimoCriado.EmprestimoId}", null);

        //    // Verifica o resultado
        //    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        //    var mensagem = await response.Content.ReadAsStringAsync();
        //    Assert.IsTrue(mensagem.Contains("Data de devolução registada com sucesso"));
        //}

        //[TestMethod]
        //public async Task ValidarEmprestimo_DeveValidarEmpréstimoComSucesso()
        //{
        //    // Simula um administrador autenticado
        //    string tokenDeAdministrador = Helpers.GerarTokenAdministrador();
        //    _httpClient.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", tokenDeAdministrador);

        //    var emprestimoId = 1; // Exemplo de um empréstimo existente

        //    // Envia uma requisição POST para o endpoint /api/Emprestimos/validar-emprestimo/{emprestimoId}
        //    var response = await _httpClient.PostAsync($"api/Emprestimos/validar-emprestimo/{emprestimoId}", null);

        //    // Verifica se o status da resposta é 200 OK
        //    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        //    // Verifica se o corpo da resposta contém a mensagem de sucesso
        //    var message = await response.Content.ReadAsStringAsync();
        //    Assert.IsTrue(message.Contains("Empréstimo validado e notificações enviadas"));
        //}

        #endregion

        #region Testes de Erro

        //[TestMethod]
        //public async Task ConcluirEmprestimo_ComUtilizadorNaoAutenticado_RetornaErro401()
        //{
        //    var emprestimoId = 1; // Exemplo de um empréstimo existente

        //    // Envia uma requisição POST para o endpoint /api/Emprestimos/devolucao-item/{emprestimoId}
        //    var response = await _httpClient.PostAsync($"api/Emprestimos/devolucao-item/{emprestimoId}", null);

        //    // Verifica se o status da resposta é 401 Unauthorized
        //    Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

        //    // Verifica se a mensagem de erro contém "Utilizador não autenticado"
        //    var errorMessage = await response.Content.ReadAsStringAsync();
        //    Assert.IsTrue(errorMessage.Contains("Utilizador não autenticado"));
        //}

        //[TestMethod]
        //public async Task ValidarEmprestimo_ComAdministradorNaoAutorizado_RetornaErro403()
        //{
        //    // Simula um utilizador normal
        //    string tokenDeUtilizador = Helpers.GerarTokenUtilizador();
        //    _httpClient.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", tokenDeUtilizador);

        //    var emprestimoId = 1; // Exemplo de um empréstimo existente

        //    // Envia uma requisição POST para o endpoint /api/Emprestimos/validar-emprestimo/{emprestimoId}
        //    var response = await _httpClient.PostAsync($"api/Emprestimos/validar-emprestimo/{emprestimoId}", null);

        //    // Verifica se o status da resposta é 403 Forbidden
        //    Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);

        //    // Verifica se a mensagem de erro contém "Apenas administradores podem validar empréstimos"
        //    var errorMessage = await response.Content.ReadAsStringAsync();
        //    Assert.IsTrue(errorMessage.Contains("Apenas administradores podem validar empréstimos"));
        //}

        //[TestMethod]
        //public async Task RejeitarEmprestimo_ComAdministradorNaoAutorizado_RetornaErro403()
        //{
        //    // Simula um utilizador normal
        //    string tokenDeUtilizador = Helpers.GerarTokenUtilizador();
        //    _httpClient.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", tokenDeUtilizador);

        //    var emprestimoId = 1; // Exemplo de um empréstimo existente

        //    // Envia uma requisição POST para o endpoint /api/Emprestimos/rejeitar-emprestimo/{emprestimoId}
        //    var response = await _httpClient.PostAsync($"api/Emprestimos/rejeitar-emprestimo/{emprestimoId}", null);

        //    // Verifica se o status da resposta é 403 Forbidden
        //    Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);

        //    // Verifica se a mensagem de erro contém "Apenas administradores podem rejeitar empréstimos"
        //    var errorMessage = await response.Content.ReadAsStringAsync();
        //    Assert.IsTrue(errorMessage.Contains("Apenas administradores podem rejeitar empréstimos"));
        //}

        //[TestMethod]
        //public async Task ValidarDevolucaoEmprestimo_ComAdministradorNaoAutorizado_RetornaErro403()
        //{
        //    // Simula um utilizador normal
        //    string tokenDeUtilizador = Helpers.GerarTokenUtilizador();
        //    _httpClient.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", tokenDeUtilizador);

        //    var emprestimoId = 1; // Exemplo de um empréstimo existente

        //    // Envia uma requisição POST para o endpoint /api/Emprestimos/validar-devolucao/{emprestimoId}
        //    var response = await _httpClient.PostAsync($"api/Emprestimos/validar-devolucao/{emprestimoId}", null);

        //    // Verifica se o status da resposta é 403 Forbidden
        //    Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);

        //    // Verifica se a mensagem de erro contém "Apenas administradores podem validar devoluções"
        //    var errorMessage = await response.Content.ReadAsStringAsync();
        //    Assert.IsTrue(errorMessage.Contains("Apenas administradores podem validar devoluções"));
        //}

        #endregion
    }
}