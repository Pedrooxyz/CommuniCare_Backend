using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;
using CommuniCare;
using System.Net;
using System.Collections.Generic;
using CommuniCare.Models;
using System.Net.Http.Json; // Para usar ReadFromJsonAsync
using CommuniCare.DTOs; // Ajuste o namespace conforme a localização de ArtigoDto
using System.Net.Http.Headers;
using System.Drawing.Printing; // Para AuthenticationHeaderValue
using Microsoft.Extensions.Configuration;


namespace CommuniCareTest
{

    [TestClass]
    public class ArtigosControllerTests
    {

        private readonly HttpClient _httpClient;

        public ArtigosControllerTests()
        {
            // Cria a instância da aplicação da Web para os testes
            var webAppFactory = new WebApplicationFactory<Program>();
            _httpClient = webAppFactory.CreateDefaultClient(); // Cria o HttpClient
        }


        #region Testes de Sucesso

        [TestMethod]
        public async Task GetArtigosDisponiveis_ReturnsDisponiveisArtigos()
        {
            // Envia uma requisição GET para o endpoint /api/Artigos/disponiveis
            var response = await _httpClient.GetAsync("api/Artigos/disponiveis");

            // Verifica se o status da resposta é 200 OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Verifica se o corpo da resposta pode ser desserializado para uma lista de Artigos
            var artigosDisponiveis = await response.Content.ReadFromJsonAsync<List<Artigo>>();

            // Verifica se a lista de artigos não é nula
            Assert.IsNotNull(artigosDisponiveis);

            // Verifica se todos os artigos têm o estado "Disponível"
            foreach (var artigo in artigosDisponiveis)
            {
                Assert.AreEqual(EstadoArtigo.Disponivel, artigo.Estado);
            }
        }

        [TestMethod]
        public async Task PublicarArtigo_CriaArtigoComSucesso()
        {
            // Prepara o objeto ArtigoDto com dados válidos
            var artigoDto = new ArtigoDto
            {
                NomeArtigo = "Novo Artigo",
                DescArtigo = "Descrição do novo artigo",
                CustoCares = 20,
                QuantidadeDisponivel =1
            };

            string tokenDeAdministrador = Helpers.GerarTokenAdministrador();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenDeAdministrador);

            // Cria o conteúdo do pedido como JSON
            var content = JsonContent.Create(artigoDto);

            // Envia uma requisição POST para o endpoint /api/Artigos/publicar
            var response = await _httpClient.PostAsync("api/Artigos/publicar", content);

            // Verifica se o status da resposta é 201 Created
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            // Verifica se o corpo da resposta pode ser desserializado para um Artigo
            var artigoCriado = await response.Content.ReadFromJsonAsync<Artigo>();

            // Verifica se o artigo foi criado corretamente
            Assert.IsNotNull(artigoCriado);
            Assert.AreEqual(artigoDto.NomeArtigo, artigoCriado.NomeArtigo);
            Assert.AreEqual(artigoDto.DescArtigo, artigoCriado.DescArtigo);
            Assert.AreEqual(artigoDto.CustoCares, artigoCriado.CustoCares);
            Assert.AreEqual(EstadoArtigo.Disponivel, artigoCriado.Estado);
        }

        #endregion

        #region Testes de Erro

        [TestMethod]
        public async Task PublicarArtigo_ComClienteNaoAutorizado_RetornaErro403()
        {
            // Prepara o objeto ArtigoDto com dados válidos
            var artigoDto = new ArtigoDto
            {
                NomeArtigo = "Novo Artigo",
                DescArtigo = "Descrição do novo artigo",
                CustoCares = 20,
                QuantidadeDisponivel = 2
            };

            // Cria o conteúdo do pedido como JSON
            var content = JsonContent.Create(artigoDto);

            // Simula um cliente autenticado com token inválido ou sem permissões
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token_valido_do_cliente");

            // Envia uma requisição POST para o endpoint /api/Artigos/publicar
            var response = await _httpClient.PostAsync("api/Artigos/publicar", content);

            // Verifica se o status da resposta é 401 Unauthorized (utilizador não autenticado)
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            // Lê a mensagem de erro
            var errorMessage = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(errorMessage.Contains("Utilizador não autenticado.") || errorMessage.Contains("Apenas administradores podem publicar artigos."));
        }

        #endregion

    }
}
