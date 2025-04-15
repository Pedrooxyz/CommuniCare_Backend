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
    public class TransacoesEmprestimoController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public TransacoesEmprestimoController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/TransacaoEmprestimos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransacaoEmprestimo>>> GetTransacaoEmprestimos()
        {
            return await _context.TransacoesEmprestimo.ToListAsync();
        }

        // GET: api/TransacaoEmprestimos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TransacaoEmprestimo>> GetTransacaoEmprestimo(int id)
        {
            var transacaoEmprestimo = await _context.TransacoesEmprestimo.FindAsync(id);

            if (transacaoEmprestimo == null)
            {
                return NotFound();
            }

            return transacaoEmprestimo;
        }

        // PUT: api/TransacaoEmprestimos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransacaoEmprestimo(int id, TransacaoEmprestimo transacaoEmprestimo)
        {
            if (id != transacaoEmprestimo.TransacaoId)
            {
                return BadRequest();
            }

            _context.Entry(transacaoEmprestimo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransacaoEmprestimoExists(id))
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

        // POST: api/TransacaoEmprestimos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TransacaoEmprestimo>> PostTransacaoEmprestimo(TransacaoEmprestimo transacaoEmprestimo)
        {
            _context.TransacoesEmprestimo.Add(transacaoEmprestimo);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TransacaoEmprestimoExists(transacaoEmprestimo.TransacaoId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTransacaoEmprestimo", new { id = transacaoEmprestimo.TransacaoId }, transacaoEmprestimo);
        }

        // DELETE: api/TransacaoEmprestimos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransacaoEmprestimo(int id)
        {
            var transacaoEmprestimo = await _context.TransacoesEmprestimo.FindAsync(id);
            if (transacaoEmprestimo == null)
            {
                return NotFound();
            }

            _context.TransacoesEmprestimo.Remove(transacaoEmprestimo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransacaoEmprestimoExists(int id)
        {
            return _context.TransacoesEmprestimo.Any(e => e.TransacaoId == id);
        }
    }
}
