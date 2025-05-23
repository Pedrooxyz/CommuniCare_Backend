using CommuniCare.DTOs;
using CommuniCare.Models;
using CommunicareIntegrationsTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;


namespace CommuniCareIntegrationsTest
{
    [TestClass]
    public class IntegrationTests
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;


        [TestInitialize]
        public void Setup()
        {
            _factory = new IntegrationWebAppFactory();
            _client = _factory.CreateClient();


            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(TestAuthHandler.Scheme);
        }

        [TestMethod]
        public async Task GET_root_returns_success_and_content()
        {
            var response = await _client.GetAsync("/api/Utilizadores");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(string.IsNullOrWhiteSpace(json));
        }

        public record PedidoAjudaDto(
            int PedidoAjudaId,
            string DescPedido,
            int NHoras,
            int NPessoas,
            DateTime HorarioAjuda,
            string FotografiaPA);


        [TestMethod]
        public async Task POST_PedidoAjuda_creates_resource_and_can_be_fetched()
        {
            var novoPedido = new
            {
                DescPedido = "Preciso de ajuda com a horta.",
                NHoras = 3,
                NPessoas = 2,
                HorarioAjuda = DateTime.UtcNow.AddDays(1),
                FotografiaPA = "horta.jpg"
            };

            var postRes = await _client.PostAsJsonAsync(
                "/api/PedidosAjuda/Pedir",
                novoPedido);

            Assert.AreEqual(HttpStatusCode.OK, postRes.StatusCode,
                $"Esperava 200 mas obtive {(int)postRes.StatusCode}");

            var listRes = await _client.GetAsync("/api/PedidosAjuda");
            listRes.EnsureSuccessStatusCode();      

            var pedidos = await listRes.Content.ReadFromJsonAsync<PedidoAjudaDto[]>();
            Assert.IsNotNull(pedidos, "Falhou a desserialização da lista de pedidos.");

            bool existe = pedidos!.Any(p =>
                p.DescPedido == novoPedido.DescPedido &&
                p.NHoras == novoPedido.NHoras &&
                p.NPessoas == novoPedido.NPessoas);

            Assert.IsTrue(existe,
                "O pedido recém-criado não foi encontrado na lista.");
        }

        [TestMethod]
        public async Task POST_CriarLoja_creates_active_store_and_deactivates_others()
        {
            var dto = new LojaDto
            {
                NomeLoja = "Loja Nova",
                DescLoja = "Agora é a ativa"
            };

            var res = await _client.PostAsJsonAsync("/api/Lojas/CriarLoja-(admin)", dto);

            Assert.AreEqual(
                HttpStatusCode.Created,
                res.StatusCode,
                $"Esperava 201 mas obtive {(int)res.StatusCode}");

            Assert.IsTrue(res.Headers.Location is not null,
                "O header Location não foi devolvido.");

            var body = await res.Content.ReadFromJsonAsync<object>();
            Assert.IsNotNull(body, "Corpo da resposta não veio.");

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CommuniCareContext>();

            var ativa = db.Lojas.SingleOrDefault(l => l.Estado == EstadoLoja.Ativo);
            var inativas = db.Lojas.Where(l => l.Estado == EstadoLoja.Inativo).ToList();

            Assert.IsNotNull(ativa, "Não existe loja ativa após a criação.");
            Assert.AreEqual("Loja Nova", ativa!.NomeLoja);

            Assert.IsTrue(inativas.Any(l => l.LojaId == 1),
                "A loja antiga não foi desativada.");
        }

        [TestMethod]
        public async Task POST_AdquirirItem_returns_BadRequest_when_insufficient_cares()
        {
            int itemId;  

            
            await using (var scope = _factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CommuniCareContext>();

                
                var buyer = await db.Utilizadores.FindAsync(2);
                if (buyer == null)
                    Assert.Inconclusive("O seed inicial precisa conter o Utilizador ID=2.");
                buyer.NumCares = 1;                

               
                var owner = await db.Utilizadores.FindAsync(1);
                if (owner == null)
                    Assert.Inconclusive("O seed inicial precisa conter o Utilizador ID=1.");

               
                var item = new ItemEmprestimo
                {
                    NomeItem = "Livro valioso (teste)",
                    Disponivel = EstadoItemEmprestimo.Disponivel,
                    ComissaoCares = 5
                };
                db.ItensEmprestimo.Add(item);
                await db.SaveChangesAsync();       
                itemId = item.ItemId;

                
                db.ItemEmprestimoUtilizadores.Add(new ItemEmprestimoUtilizador
                {
                    ItemId = itemId,
                    UtilizadorId = owner.UtilizadorId,
                    TipoRelacao = "Dono"
                });
                await db.SaveChangesAsync();     
            }

            
            var response = await _client.PostAsync(
                $"/api/ItensEmprestimo/AdquirirItem/{itemId}", null);

            
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            StringAssert.Contains(body,
                "Saldo de Cares insuficiente para adquirir este item.",
                "A API devia recusar quando o utilizador não tem Cares.");
        }
    }




}
