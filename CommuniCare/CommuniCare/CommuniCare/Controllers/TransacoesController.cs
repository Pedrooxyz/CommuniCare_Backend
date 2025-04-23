using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommuniCare.Models;

namespace CommuniCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransacoesController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public TransacoesController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/Transacoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transacao>>> GetTransacaos()
        {
            return await _context.Transacoes.ToListAsync();
        }

        // GET: api/Transacoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transacao>> GetTransacao(int id)
        {
            var transacao = await _context.Transacoes.FindAsync(id);

            if (transacao == null)
            {
                return NotFound();
            }

            return transacao;
        }

        // PUT: api/Transacoes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransacao(int id, Transacao transacao)
        {
            if (id != transacao.TransacaoId)
            {
                return BadRequest();
            }

            _context.Entry(transacao).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransacaoExists(id))
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

        // POST: api/Transacoes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Transacao>> PostTransacao(Transacao transacao)
        {
            _context.Transacoes.Add(transacao);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTransacao", new { id = transacao.TransacaoId }, transacao);
        }

        // DELETE: api/Transacoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransacao(int id)
        {
            var transacao = await _context.Transacoes.FindAsync(id);
            if (transacao == null)
            {
                return NotFound();
            }

            _context.Transacoes.Remove(transacao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransacaoExists(int id)
        {
            return _context.Transacoes.Any(e => e.TransacaoId == id);
        }


        [HttpGet("Historico/{utilizadorId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetHistoricoTransacoes(int utilizadorId)
        {
            var transacoesAjuda = await _context.Transacoes
                .Where(t => t.TransacaoAjuda != null && t.TransacaoAjuda.RecetorTran == utilizadorId)
                .ToListAsync();

            var transacoesEmprestimo = await _context.Transacoes
                .Where(t => t.TransacaoEmprestimo != null &&
                       (t.TransacaoEmprestimo.RecetorTran == utilizadorId || t.TransacaoEmprestimo.PagaTran == utilizadorId))
                .ToListAsync();

            var transacoesVenda = await _context.Transacoes
                .Where(t => t.Venda != null && t.Venda.UtilizadorId == utilizadorId)
                .ToListAsync();

            var historico = transacoesAjuda
                .Select(t => new
                {
                    t.TransacaoId,
                    Tipo = "Ajuda",
                    Data = ((DateTime)t.DataTransacao).ToString("dd/MM HH:mm"),
                    NumeroCarenciasTransferido = t.Quantidade
                })
                .Concat(transacoesEmprestimo.Select(t => new
                {
                    t.TransacaoId,
                    Tipo = "Emprestimo",
                    Data = ((DateTime)t.DataTransacao).ToString("dd/MM HH:mm"),
                    NumeroCarenciasTransferido = t.Quantidade
                }))
                .Concat(transacoesVenda.Select(t => new
                {
                    t.TransacaoId,
                    Tipo = "Venda",
                    Data = ((DateTime)t.DataTransacao).ToString("dd/MM HH:mm"),
                    NumeroCarenciasTransferido = t.Quantidade
                }))
                .OrderByDescending(t => t.Data)
                .ToList();

            return historico;
        }
    }
}
