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
    public class NotificacoesController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public NotificacoesController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/Notificacoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notificacao>>> GetNotificacaos()
        {
            return await _context.Notificacaos.ToListAsync();
        }

        // GET: api/Notificacoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Notificacao>> GetNotificacao(int id)
        {
            var notificacao = await _context.Notificacaos.FindAsync(id);

            if (notificacao == null)
            {
                return NotFound();
            }

            return notificacao;
        }

        // PUT: api/Notificacoes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNotificacao(int id, Notificacao notificacao)
        {
            if (id != notificacao.NotificacaoId)
            {
                return BadRequest();
            }

            _context.Entry(notificacao).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificacaoExists(id))
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

        // POST: api/Notificacoes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Notificacao>> PostNotificacao(Notificacao notificacao)
        {
            _context.Notificacaos.Add(notificacao);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNotificacao", new { id = notificacao.NotificacaoId }, notificacao);
        }

        // DELETE: api/Notificacoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotificacao(int id)
        {
            var notificacao = await _context.Notificacaos.FindAsync(id);
            if (notificacao == null)
            {
                return NotFound();
            }

            _context.Notificacaos.Remove(notificacao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NotificacaoExists(int id)
        {
            return _context.Notificacaos.Any(e => e.NotificacaoId == id);
        }
    }
}
