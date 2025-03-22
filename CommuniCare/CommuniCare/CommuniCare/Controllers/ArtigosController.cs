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
    public class ArtigosController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public ArtigosController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/Artigos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Artigo>>> GetArtigos()
        {
            return await _context.Artigos.ToListAsync();
        }

        // GET: api/Artigos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Artigo>> GetArtigo(int id)
        {
            var artigo = await _context.Artigos.FindAsync(id);

            if (artigo == null)
            {
                return NotFound();
            }

            return artigo;
        }

        // PUT: api/Artigos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArtigo(int id, Artigo artigo)
        {
            if (id != artigo.ArtigoId)
            {
                return BadRequest();
            }

            _context.Entry(artigo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArtigoExists(id))
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

        // POST: api/Artigos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Artigo>> PostArtigo(Artigo artigo)
        {
            _context.Artigos.Add(artigo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArtigo", new { id = artigo.ArtigoId }, artigo);
        }

        // DELETE: api/Artigos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArtigo(int id)
        {
            var artigo = await _context.Artigos.FindAsync(id);
            if (artigo == null)
            {
                return NotFound();
            }

            _context.Artigos.Remove(artigo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ArtigoExists(int id)
        {
            return _context.Artigos.Any(e => e.ArtigoId == id);
        }
    }
}
