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
    public class PedidosAjudaController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public PedidosAjudaController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/PedidoAjudas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoAjuda>>> GetPedidoAjuda()
        {
            return await _context.PedidoAjuda.ToListAsync();
        }

        // GET: api/PedidoAjudas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoAjuda>> GetPedidoAjuda(int id)
        {
            var pedidoAjuda = await _context.PedidoAjuda.FindAsync(id);

            if (pedidoAjuda == null)
            {
                return NotFound();
            }

            return pedidoAjuda;
        }

        // PUT: api/PedidoAjudas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedidoAjuda(int id, PedidoAjuda pedidoAjuda)
        {
            if (id != pedidoAjuda.PedidoId)
            {
                return BadRequest();
            }

            _context.Entry(pedidoAjuda).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PedidoAjudaExists(id))
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

        // POST: api/PedidoAjudas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PedidoAjuda>> PostPedidoAjuda(PedidoAjuda pedidoAjuda)
        {
            _context.PedidoAjuda.Add(pedidoAjuda);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPedidoAjuda", new { id = pedidoAjuda.PedidoId }, pedidoAjuda);
        }

        // DELETE: api/PedidoAjudas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedidoAjuda(int id)
        {
            var pedidoAjuda = await _context.PedidoAjuda.FindAsync(id);
            if (pedidoAjuda == null)
            {
                return NotFound();
            }

            _context.PedidoAjuda.Remove(pedidoAjuda);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PedidoAjudaExists(int id)
        {
            return _context.PedidoAjuda.Any(e => e.PedidoId == id);
        }
    }
}
