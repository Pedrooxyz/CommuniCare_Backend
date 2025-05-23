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
        #region CONTROLLERS AUTOMÁTICOS
        ///// <summary>
        ///// Obtém os detalhes de uma venda específica com base no ID.
        ///// </summary>
        ///// <param name="id">ID da venda.</param>
        ///// <returns>Retorna os detalhes da venda ou 404 Not Found se a venda não for encontrada.</returns>
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Venda>> GetVenda(int id)
        //{
        //    var venda = await _context.Venda.FindAsync(id);

        //    if (venda == null)
        //    {
        //        return NotFound();
        //    }

        //    return venda;
        //}

        ///// <summary>
        ///// Atualiza os dados de uma venda existente.
        ///// </summary>
        ///// <param name="id">ID da venda a ser atualizada.</param>
        ///// <param name="venda">Objeto contendo os novos dados da venda.</param>
        ///// <returns>Retorna 204 No Content se a venda for atualizada com sucesso, ou 400 Bad Request se houver inconsistências.</returns>
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutVenda(int id, Venda venda)
        //{
        //    if (id != venda.TransacaoId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(venda).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!VendaExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        ///// <summary>
        ///// Cria uma nova venda no sistema.
        ///// </summary>
        ///// <param name="venda">Objeto contendo os dados da nova venda.</param>
        ///// <returns>Retorna 201 Created se a venda for criada com sucesso, ou 409 Conflict se já existir uma venda com o mesmo ID.</returns>
        //[HttpPost]
        //public async Task<ActionResult<Venda>> PostVenda(Venda venda)
        //{
        //    _context.Venda.Add(venda);
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        if (VendaExists(venda.TransacaoId))
        //        {
        //            return Conflict();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return CreatedAtAction("GetVenda", new { id = venda.TransacaoId }, venda);
        //}

        ///// <summary>
        ///// Exclui uma venda existente com base no ID.
        ///// </summary>
        ///// <param name="id">ID da venda a ser excluída.</param>
        ///// <returns>Retorna 204 No Content se a venda for excluída com sucesso, ou 404 Not Found se a venda não for encontrada.</returns>
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteVenda(int id)
        //{
        //    var venda = await _context.Venda.FindAsync(id);
        //    if (venda == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Venda.Remove(venda);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        #endregion

        private bool VendaExists(int id)
        {
            return _context.Venda.Any(e => e.TransacaoId == id);
        }

        [HttpPost("Comprar")]
        [Authorize]
        public async Task<IActionResult> Comprar([FromBody] PedidoCompraDTO request)
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
                    return BadRequest(new { Sucesso = false, Erro = "Artigos indisponíveis." });

                if (artigosDisponiveis.Any(a => a.Loja == null || a.Loja.Estado != EstadoLoja.Ativo))
                    return BadRequest(new { Sucesso = false, Erro = "Artigos pertencem a lojas inativas." });

                var (venda, transacao, artigos, dataCompra) = await _transacaoServico.ProcessarCompraAsync(userId, request.ArtigosIds);

                foreach (var artigo in artigos)
                    artigo.QuantidadeDisponivel -= 1;

                transacao.Quantidade = artigos.Sum(a => a.CustoCares ?? 0);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = "Compra realizada com sucesso.",
                    TransacaoId = transacao.TransacaoId,
                    DataHora = dataCompra.ToString("yyyy-MM-dd HH:mm"),
                    NomeArtigos = artigos.Select(a => a.NomeArtigo)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Sucesso = false, Erro = ex.Message });
            }
        }

        [HttpGet("Comprovativo/Email/{transacaoId}")]
        [Authorize]
        public async Task<IActionResult> EnviarComprovativoEmail(int transacaoId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            try
            {
                var venda = await _context.Venda
                    .Include(v => v.Artigos)
                    .Include(v => v.Transacao)
                    .FirstOrDefaultAsync(v => v.TransacaoId == transacaoId && v.UtilizadorId == userId);

                if (venda == null)
                    return NotFound(new { Sucesso = false, Erro = "Venda não encontrada." });

                var user = await _context.Utilizadores.FindAsync(userId);
                var emailContacto = await _context.Contactos
                    .Where(c => c.UtilizadorId == userId && c.TipoContactoId == 1)
                    .Select(c => c.NumContacto)
                    .FirstOrDefaultAsync();

                var comprovativo = ComprovativoGenerator.GerarComprovativoUnicoPDF(venda, user, venda.Artigos.ToList());

                await _emailService.EnviarComprovativoCompra(emailContacto, user.NomeUtilizador, comprovativo);

                return Ok(new { Sucesso = true, Mensagem = "Comprovativo enviado por email com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Sucesso = false, Erro = ex.Message });
            }
        }

        [HttpGet("Comprovativo/Download/{transacaoId}")]
        [Authorize]
        public async Task<IActionResult> DownloadComprovativo(int transacaoId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            try
            {
                var venda = await _context.Venda
                    .Include(v => v.Artigos)
                    .Include(v => v.Transacao)
                    .FirstOrDefaultAsync(v => v.TransacaoId == transacaoId && v.UtilizadorId == userId);

                if (venda == null)
                    return NotFound(new { Sucesso = false, Erro = "Venda não encontrada." });

                var user = await _context.Utilizadores.FindAsync(userId);

                var comprovativo = ComprovativoGenerator.GerarComprovativoUnicoPDF(venda, user, venda.Artigos.ToList());

                return File(comprovativo, "application/pdf", "ComprovativoCompra.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Sucesso = false, Erro = ex.Message });
            }
        }




    }
}

