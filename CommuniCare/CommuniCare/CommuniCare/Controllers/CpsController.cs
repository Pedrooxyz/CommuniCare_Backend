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
    public class CpsController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public CpsController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/Cps
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cp>>> GetCps()
        {
            return await _context.Cps.ToListAsync();
        }

        // GET: api/Cps/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cp>> GetCp(int id)
        {
            var cp = await _context.Cps.FindAsync(id);

            if (cp == null)
            {
                return NotFound();
            }

            return cp;
        }

        // PUT: api/Cps/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCp(int id, Cp cp)
        {
            if (id != cp.Cpid)
            {
                return BadRequest();
            }

            _context.Entry(cp).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CpExists(id))
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

        // POST: api/Cps
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Cp>> PostCp(Cp cp)
        {
            _context.Cps.Add(cp);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCp", new { id = cp.Cpid }, cp);
        }

        // DELETE: api/Cps/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCp(int id)
        {
            var cp = await _context.Cps.FindAsync(id);
            if (cp == null)
            {
                return NotFound();
            }

            _context.Cps.Remove(cp);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CpExists(int id)
        {
            return _context.Cps.Any(e => e.Cpid == id);
        }
    }
}
