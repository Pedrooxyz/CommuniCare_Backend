using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommuniCare.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        [HttpPost("rejeitar-voluntario/{idVoluntariado}")]
        [Authorize]
        public async Task<IActionResult> RejeitarVoluntario(int idVoluntariado)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int adminId = int.Parse(userIdClaim.Value);

            var admin = await _context.Utilizadores.FindAsync(adminId);
            if (admin == null || admin.TipoUtilizadorId != 2)
            {
                return Forbid("Apenas administradores podem rejeitar voluntários.");
            }

            var voluntariado = await _context.Voluntariados
                .Include(v => v.Pedido)
                .Include(v => v.Utilizador)
                .FirstOrDefaultAsync(v => v.IdVoluntariado == idVoluntariado && v.Estado == EstadoVoluntariado.Pendente);

            if (voluntariado == null)
            {
                return NotFound("Voluntariado não encontrado ou já foi processado.");
            }

            // Remover ou marcar como rejeitado
            _context.Voluntariados.Remove(voluntariado);

            var notificacaoVoluntario = new Notificacao
            {
                Mensagem = $"A tua candidatura como voluntário para o pedido #{voluntariado.Pedido.PedidoId} foi rejeitada.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                PedidoId = voluntariado.PedidoId,
                UtilizadorId = voluntariado.UtilizadorId,
                ItemId = null
            };

            _context.Notificacaos.Add(notificacaoVoluntario);
            await _context.SaveChangesAsync();

            return Ok("Voluntário rejeitado com sucesso.");
        }

        [HttpPost("aceitar-voluntario/{idVoluntariado}")]
        [Authorize]
        public async Task<IActionResult> AceitarVoluntario(int idVoluntariado)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int adminId = int.Parse(userIdClaim.Value);

            // Verificar se o utilizador é administrador
            var utilizador = await _context.Utilizadores.FindAsync(adminId);
            if (utilizador == null || utilizador.TipoUtilizadorId != 2)
            {
                return Forbid("Apenas administradores podem aceitar voluntários.");
            }

            var voluntariado = await _context.Voluntariados
                .Include(v => v.Pedido)
                .Include(v => v.Pedido.Voluntariados)
                .Include(v => v.Pedido.Utilizador)
                .FirstOrDefaultAsync(v => v.IdVoluntariado == idVoluntariado && v.Estado == EstadoVoluntariado.Pendente);

            if (voluntariado == null)
            {
                return BadRequest("Voluntariado não encontrado ou não está pendente.");
            }

            // Aceitar voluntário
            voluntariado.Estado = EstadoVoluntariado.Aceite;

            var pedido = voluntariado.Pedido;

            // Verificar se o número de voluntários aceites atinge o limite
            int voluntariosAceites = pedido.Voluntariados.Count(v => v.Estado == EstadoVoluntariado.Aceite);
            if (pedido.NPessoas.HasValue && voluntariosAceites >= pedido.NPessoas.Value)
            {
                pedido.Estado = EstadoPedido.EmProgresso;
            }

            // Notificação ao voluntário aceite
            var notificacaoVoluntario = new Notificacao
            {
                Mensagem = $"Foste aceite como voluntário para o pedido #{pedido.PedidoId}.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                PedidoId = pedido.PedidoId,
                UtilizadorId = voluntariado.UtilizadorId,
                ItemId = null
            };
            _context.Notificacaos.Add(notificacaoVoluntario);

            // Notificação ao requisitante
            var notificacaoRequisitante = new Notificacao
            {
                Mensagem = $"Um voluntário foi aceite para o teu pedido #{pedido.PedidoId}.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                PedidoId = pedido.PedidoId,
                UtilizadorId = pedido.UtilizadorId,
                ItemId = null
            };
            _context.Notificacaos.Add(notificacaoRequisitante);

            await _context.SaveChangesAsync();

            return Ok("Voluntário aceite com sucesso" + (pedido.Estado == EstadoPedido.EmProgresso ? " e pedido atualizado para 'Em Progresso'." : "."));
        }



    }
}
