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
    public class TipoUtilizadoresController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public TipoUtilizadoresController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/TipoUtilizadores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoUtilizador>>> GetTipoUtilizadors()
        {
            return await _context.TipoUtilizadors.ToListAsync();
        }

        // GET: api/TipoUtilizadores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoUtilizador>> GetTipoUtilizador(int id)
        {
            var tipoUtilizador = await _context.TipoUtilizadors.FindAsync(id);

            if (tipoUtilizador == null)
            {
                return NotFound();
            }

            return tipoUtilizador;
        }

        // PUT: api/TipoUtilizadores/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoUtilizador(int id, TipoUtilizador tipoUtilizador)
        {
            if (id != tipoUtilizador.TipoUtilizadorId)
            {
                return BadRequest();
            }

            _context.Entry(tipoUtilizador).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoUtilizadorExists(id))
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

        // POST: api/TipoUtilizadores
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TipoUtilizador>> PostTipoUtilizador(TipoUtilizador tipoUtilizador)
        {
            _context.TipoUtilizadors.Add(tipoUtilizador);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTipoUtilizador", new { id = tipoUtilizador.TipoUtilizadorId }, tipoUtilizador);
        }

        // DELETE: api/TipoUtilizadores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoUtilizador(int id)
        {
            var tipoUtilizador = await _context.TipoUtilizadors.FindAsync(id);
            if (tipoUtilizador == null)
            {
                return NotFound();
            }

            _context.TipoUtilizadors.Remove(tipoUtilizador);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TipoUtilizadorExists(int id)
        {
            return _context.TipoUtilizadors.Any(e => e.TipoUtilizadorId == id);
        }
    }
}
