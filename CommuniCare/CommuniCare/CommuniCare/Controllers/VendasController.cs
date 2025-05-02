using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommuniCare.Models;
using Microsoft.AspNetCore.Authorization;
using CommuniCare.DTOs;
using System.Security.Claims;

namespace CommuniCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendasController : ControllerBase
    {
        private readonly CommuniCareContext _context;
        private readonly EmailService _emailService;
        private readonly TransacaoServico _transacaoServico;
        public VendasController(CommuniCareContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
            _transacaoServico = new TransacaoServico(_context);
        }

        /// <summary>
        /// Obtém todas as vendas registradas no sistema.
        /// </summary>
        /// <returns>Retorna uma lista de vendas ou 404 Not Found se não houver vendas registradas.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venda>>> GetVenda()
        {
            return await _context.Venda.ToListAsync();
        }

        /// <summary>
        /// Obtém os detalhes de uma venda específica com base no ID.
        /// </summary>
        /// <param name="id">ID da venda.</param>
        /// <returns>Retorna os detalhes da venda ou 404 Not Found se a venda não for encontrada.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Venda>> GetVenda(int id)
        {
            var venda = await _context.Venda.FindAsync(id);

            if (venda == null)
            {
                return NotFound();
            }

            return venda;
        }

        /// <summary>
        /// Atualiza os dados de uma venda existente.
        /// </summary>
        /// <param name="id">ID da venda a ser atualizada.</param>
        /// <param name="venda">Objeto contendo os novos dados da venda.</param>
        /// <returns>Retorna 204 No Content se a venda for atualizada com sucesso, ou 400 Bad Request se houver inconsistências.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenda(int id, Venda venda)
        {
            if (id != venda.TransacaoId)
            {
                return BadRequest();
            }

            _context.Entry(venda).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Cria uma nova venda no sistema.
        /// </summary>
        /// <param name="venda">Objeto contendo os dados da nova venda.</param>
        /// <returns>Retorna 201 Created se a venda for criada com sucesso, ou 409 Conflict se já existir uma venda com o mesmo ID.</returns>
        [HttpPost]
        public async Task<ActionResult<Venda>> PostVenda(Venda venda)
        {
            _context.Venda.Add(venda);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (VendaExists(venda.TransacaoId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetVenda", new { id = venda.TransacaoId }, venda);
        }

        /// <summary>
        /// Exclui uma venda existente com base no ID.
        /// </summary>
        /// <param name="id">ID da venda a ser excluída.</param>
        /// <returns>Retorna 204 No Content se a venda for excluída com sucesso, ou 404 Not Found se a venda não for encontrada.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenda(int id)
        {
            var venda = await _context.Venda.FindAsync(id);
            if (venda == null)
            {
                return NotFound();
            }

            _context.Venda.Remove(venda);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VendaExists(int id)
        {
            return _context.Venda.Any(e => e.TransacaoId == id);
        }


        /// <summary>
        /// Realiza a compra de artigos para o utilizador autenticado.
        /// </summary>
        /// <param name="request">Objeto contendo os IDs dos artigos a serem comprados.</param>
        /// <returns>Retorna 200 OK se a compra for processada com sucesso, ou 400 Bad Request em caso de erro.</returns>
        [HttpPost("comprar")]
        [Authorize]
        public async Task<IActionResult> Comprar([FromBody] PedidoCompraDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            try
            {
                await _transacaoServico.ProcessarCompraAsync(userId, request.ArtigosIds);
                return Ok(new { Sucesso = true, Mensagem = "Compra efetuada com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Sucesso = false, Erro = ex.Message });
            }
        }

        /// <summary>
        /// Realiza a compra de artigos para o utilizador autenticado e envia o comprovativo por email.
        /// </summary>
        /// <param name="request">Objeto contendo os IDs dos artigos a serem comprados.</param>
        /// <returns>Retorna 200 OK se a compra for processada com sucesso e o comprovativo enviado por e-mail, ou 400 Bad Request em caso de erro.</returns>
        [HttpPost("comprar-email")]
        [Authorize]
        public async Task<IActionResult> ComprarEmail([FromBody] PedidoCompraDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            try
            {
                var user = await _context.Utilizadores.FindAsync(userId);
                var emailContacto = await _context.Contactos
                    .Where(c => c.UtilizadorId == userId && c.TipoContactoId == 1)
                    .Select(c => c.NumContacto)
                    .FirstOrDefaultAsync();

                var artigosDisponiveis = await _context.Artigos
                    .Include(a => a.Loja)
                    .Where(a => request.ArtigosIds.Contains(a.ArtigoId))
                    .ToListAsync();

                if (artigosDisponiveis.Any(a => a.QuantidadeDisponivel <= 0))
                {
                    return BadRequest(new
                    {
                        Sucesso = false,
                        Erro = "Um ou mais artigos selecionados não estão disponíveis no momento."
                    });
                }

                if (artigosDisponiveis.Any(a => a.Loja == null || a.Loja.Estado != EstadoLoja.Ativo))
                {
                    return BadRequest(new
                    {
                        Sucesso = false,
                        Erro = "Um ou mais artigos pertencem a lojas que não estão ativas no momento."
                    });
                }

                var (venda, transacao, artigos, dataCompra) = await _transacaoServico.ProcessarCompraAsync(userId, request.ArtigosIds);

                foreach (var artigo in artigos)
                {
                    artigo.QuantidadeDisponivel -= 1;
                }

                transacao.Quantidade = artigos.Sum(a => a.CustoCares ?? 0);
                await _context.SaveChangesAsync();

                var comprovativoUnico = ComprovativoGenerator.GerarComprovativoUnicoPDF(venda, user, artigos);

                await _emailService.EnviarComprovativoCompra(emailContacto, user.NomeUtilizador, comprovativoUnico);

                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = "Compra efetuada e comprovativo enviado por email.",
                    DataHora = dataCompra.ToString("yyyy-MM-dd HH:mm"),
                    NomeArtigos = artigos.Select(a => a.NomeArtigo)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Sucesso = false, Erro = ex.Message });
            }
        }

        /// <summary>
        /// Realiza a compra de artigos para o utilizador autenticado e permite o download do comprovativo em PDF.
        /// </summary>
        /// <param name="request">Objeto contendo os IDs dos artigos a serem comprados.</param>
        /// <returns>Retorna 200 OK com o comprovativo de compra em PDF ou 400 Bad Request em caso de erro.</returns>
        [HttpPost("comprar-download")]
        [Authorize]
        public async Task<IActionResult> ComprarDownload([FromBody] PedidoCompraDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            try
            {
                var user = await _context.Utilizadores.FindAsync(userId);

                var artigosDisponiveis = await _context.Artigos
                    .Include(a => a.Loja)
                    .Where(a => request.ArtigosIds.Contains(a.ArtigoId))
                    .ToListAsync();

                if (artigosDisponiveis.Any(a => a.QuantidadeDisponivel <= 0))
                {
                    return BadRequest(new
                    {
                        Sucesso = false,
                        Erro = "Um ou mais artigos selecionados não estão disponíveis no momento."
                    });
                }

                if (artigosDisponiveis.Any(a => a.Loja == null || a.Loja.Estado != EstadoLoja.Ativo))
                {
                    return BadRequest(new
                    {
                        Sucesso = false,
                        Erro = "Um ou mais artigos pertencentes à lojas que não estão ativas no momento."
                    });
                }

                var (venda, transacao, artigos, dataCompra) = await _transacaoServico.ProcessarCompraAsync(userId, request.ArtigosIds);

                foreach (var artigo in artigos)
                {
                    artigo.QuantidadeDisponivel -= 1;
                }

                transacao.Quantidade = artigos.Sum(a => a.CustoCares ?? 0);
                await _context.SaveChangesAsync();

                var comprovativoUnico = ComprovativoGenerator.GerarComprovativoUnicoPDF(venda, user, artigos);

                Response.Headers.Add("X-DataCompra", dataCompra.ToString("yyyy-MM-dd HH:mm"));
                Response.Headers.Add("X-Artigos", string.Join(", ", artigos.Select(a => a.NomeArtigo)));

                return File(comprovativoUnico, "application/pdf", $"Vouchers_Compra.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Sucesso = false, Erro = ex.Message });
            }
        }

    }
}

