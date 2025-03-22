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
    public class TransacoesAjudaController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public TransacoesAjudaController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/TransacaoAjudas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransacaoAjuda>>> GetTransacaoAjuda()
        {
            return await _context.TransacaoAjuda.ToListAsync();
        }

        // GET: api/TransacaoAjudas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TransacaoAjuda>> GetTransacaoAjuda(int id)
        {
            var transacaoAjuda = await _context.TransacaoAjuda.FindAsync(id);

            if (transacaoAjuda == null)
            {
                return NotFound();
            }

            return transacaoAjuda;
        }

        // PUT: api/TransacaoAjudas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransacaoAjuda(int id, TransacaoAjuda transacaoAjuda)
        {
            if (id != transacaoAjuda.TransacaoId)
            {
                return BadRequest();
            }

            _context.Entry(transacaoAjuda).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransacaoAjudaExists(id))
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

        // POST: api/TransacaoAjudas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TransacaoAjuda>> PostTransacaoAjuda(TransacaoAjuda transacaoAjuda)
        {
            _context.TransacaoAjuda.Add(transacaoAjuda);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TransacaoAjudaExists(transacaoAjuda.TransacaoId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTransacaoAjuda", new { id = transacaoAjuda.TransacaoId }, transacaoAjuda);
        }

        // DELETE: api/TransacaoAjudas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransacaoAjuda(int id)
        {
            var transacaoAjuda = await _context.TransacaoAjuda.FindAsync(id);
            if (transacaoAjuda == null)
            {
                return NotFound();
            }

            _context.TransacaoAjuda.Remove(transacaoAjuda);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransacaoAjudaExists(int id)
        {
            return _context.TransacaoAjuda.Any(e => e.TransacaoId == id);
        }
    }
}
