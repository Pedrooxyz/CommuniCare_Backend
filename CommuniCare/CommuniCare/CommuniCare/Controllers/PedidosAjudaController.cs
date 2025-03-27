using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommuniCare.Models;
using CommuniCare.DTOs;

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
            return await _context.PedidosAjuda.ToListAsync();
        }

        // GET: api/PedidoAjudas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoAjuda>> GetPedidoAjuda(int id)
        {
            var pedidoAjuda = await _context.PedidosAjuda.FindAsync(id);

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
            _context.PedidosAjuda.Add(pedidoAjuda);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPedidoAjuda", new { id = pedidoAjuda.PedidoId }, pedidoAjuda);
        }

        // DELETE: api/PedidoAjudas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedidoAjuda(int id)
        {
            var pedidoAjuda = await _context.PedidosAjuda.FindAsync(id);
            if (pedidoAjuda == null)
            {
                return NotFound();
            }

            _context.PedidosAjuda.Remove(pedidoAjuda);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PedidoAjudaExists(int id)
        {
            return _context.PedidosAjuda.Any(e => e.PedidoId == id);
        }


        [HttpPost("pedir")]
        public async Task<IActionResult> PedirAjuda([FromBody] PedidoAjudaDTO pedidoData)
        {
            if (pedidoData == null)
            {
                return BadRequest("Dados inválidos.");
            }

            var utilizador = await _context.Utilizadores.FindAsync(pedidoData.UtilizadorId);
            if (utilizador == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            bool pedidoCriado = utilizador.PedirAjuda(
                pedidoData.DescPedido,
                pedidoData.HorarioAjuda,
                pedidoData.NHoras,
                pedidoData.NPessoas,
                pedidoData.UtilizadorId
            );

            if (!pedidoCriado)
            {
                return StatusCode(500, "Erro ao criar o pedido de ajuda.");
            }

            await _context.SaveChangesAsync();

            return Ok("Pedido de ajuda criado com sucesso.");
        }

        [HttpPost("{pedidoId}/voluntariar/{utilizadorId}")]
        public async Task<IActionResult> Voluntariar(int pedidoId, int utilizadorId)
        {
            var pedido = await _context.PedidosAjuda.FindAsync(pedidoId);
            if (pedido == null || pedido.Estado != EstadoPedido.Aberto)
            {
                return BadRequest("Pedido não encontrado ou já fechado.");
            }

            // Verifica se o utilizador já se voluntariou
            bool jaVoluntariado = await _context.Voluntariados
                .AnyAsync(v => v.PedidoId == pedidoId && v.UtilizadorId == utilizadorId);
            if (jaVoluntariado)
            {
                return BadRequest("Utilizador já se voluntariou para este pedido.");
            }

            // Adiciona o voluntário
            var voluntariado = new Voluntariado
            {
                PedidoId = pedidoId,
                UtilizadorId = utilizadorId
            };

            _context.Voluntariados.Add(voluntariado);
            await _context.SaveChangesAsync();

            return Ok("Utilizador voluntariado com sucesso.");
        }

        [HttpPost("{pedidoId}/aceitar/{voluntarioId}")]
        public async Task<IActionResult> AceitarVoluntario(int pedidoId, int voluntarioId)
        {
            var pedido = await _context.PedidosAjuda.Include(p => p.Voluntariados)
                                                    .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);
            if (pedido == null || pedido.Estado != EstadoPedido.Aberto)
            {
                return BadRequest("Pedido não encontrado ou já fechado.");
            }

            var voluntariado = pedido.Voluntariados.FirstOrDefault(v => v.UtilizadorId == voluntarioId);
            if (voluntariado == null)
            {
                return BadRequest("Este utilizador não se voluntariou para este pedido.");
            }

            pedido.Estado = EstadoPedido.EmProgresso;

            await _context.SaveChangesAsync();

            return Ok("Voluntário aceito com sucesso.");
        }

    }
}
