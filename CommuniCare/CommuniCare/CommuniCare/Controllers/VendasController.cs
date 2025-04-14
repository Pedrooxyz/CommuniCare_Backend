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

        // GET: api/Vendas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venda>>> GetVenda()
        {
            return await _context.Venda.ToListAsync();
        }

        // GET: api/Vendas/5
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

        // PUT: api/Vendas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

        // POST: api/Vendas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

        // DELETE: api/Vendas/5
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



                var (venda, transacao, artigos, dataCompra) = await _transacaoServico.ProcessarCompraAsync(userId, request.ArtigosIds);

                // Gerar um único PDF com todos os artigos
                var comprovativoUnico = ComprovativoGenerator.GerarComprovativoUnicoPDF(venda, user, artigos);

                // Enviar o PDF por email
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


        [HttpPost("comprar-download")]
        [Authorize]
        public async Task<IActionResult> ComprarDownload([FromBody] PedidoCompraDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            try
            {
                var user = await _context.Utilizadores.FindAsync(userId);

                var (venda, transacao, artigos, dataCompra) = await _transacaoServico.ProcessarCompraAsync(userId, request.ArtigosIds);

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
