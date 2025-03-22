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
    public class VoluntariadosController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public VoluntariadosController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/Voluntariadoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Voluntariado>>> GetVoluntariados()
        {
            return await _context.Voluntariados.ToListAsync();
        }

        // GET: api/Voluntariadoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Voluntariado>> GetVoluntariado(int id)
        {
            var voluntariado = await _context.Voluntariados.FindAsync(id);

            if (voluntariado == null)
            {
                return NotFound();
            }

            return voluntariado;
        }

        // PUT: api/Voluntariadoes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVoluntariado(int id, Voluntariado voluntariado)
        {
            if (id != voluntariado.PedidoId)
            {
                return BadRequest();
            }

            _context.Entry(voluntariado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VoluntariadoExists(id))
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

        // POST: api/Voluntariadoes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Voluntariado>> PostVoluntariado(Voluntariado voluntariado)
        {
            _context.Voluntariados.Add(voluntariado);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (VoluntariadoExists(voluntariado.PedidoId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetVoluntariado", new { id = voluntariado.PedidoId }, voluntariado);
        }

        // DELETE: api/Voluntariadoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVoluntariado(int id)
        {
            var voluntariado = await _context.Voluntariados.FindAsync(id);
            if (voluntariado == null)
            {
                return NotFound();
            }

            _context.Voluntariados.Remove(voluntariado);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VoluntariadoExists(int id)
        {
            return _context.Voluntariados.Any(e => e.PedidoId == id);
        }
    }
}
