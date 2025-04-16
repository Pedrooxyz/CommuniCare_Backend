using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommuniCare.Models;
using CommuniCare.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
        [Authorize]
        public async Task<IActionResult> CriarPedidoAjuda([FromBody] PedidoAjudaDTO pedidoData)
        {
            if (pedidoData == null ||
                string.IsNullOrWhiteSpace(pedidoData.DescPedido) ||
                pedidoData.NHoras <= 0 ||
                pedidoData.NPessoas <= 0)
            {
                return BadRequest("Dados inválidos.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            var pedido = new PedidoAjuda
            {
                DescPedido = pedidoData.DescPedido,
                HorarioAjuda = pedidoData.HorarioAjuda,
                NHoras = pedidoData.NHoras,
                NPessoas = pedidoData.NPessoas,
                UtilizadorId = utilizadorId,
                Estado = EstadoPedido.Pendente
            };

            _context.PedidosAjuda.Add(pedido);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensagem = "Pedido de ajuda criado com sucesso."
            });
        }


        [HttpPost("{pedidoId}/voluntariar")]
        [Authorize]
        public async Task<IActionResult> Voluntariar(int pedidoId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var pedido = await _context.PedidosAjuda.FindAsync(pedidoId);
            if (pedido == null || pedido.Estado != EstadoPedido.Aberto)
            {
                return BadRequest("Pedido não encontrado ou já fechado.");
            }

            bool jaVoluntariado = await _context.Voluntariados
                .AnyAsync(v => v.PedidoId == pedidoId && v.UtilizadorId == utilizadorId);

            if (jaVoluntariado)
            {
                return BadRequest("Utilizador já se voluntariou para este pedido.");
            }

            var voluntariado = new Voluntariado
            {
                PedidoId = pedidoId,
                UtilizadorId = utilizadorId
            };

            _context.Voluntariados.Add(voluntariado);
            await _context.SaveChangesAsync();

            return Ok("Utilizador voluntariado com sucesso.");
        }

        #region Administrador

        [HttpPost("{pedidoId}/aceitar/{voluntarioId}")]
        [Authorize]
        public async Task<IActionResult> AceitarVoluntario(int pedidoId, int voluntarioId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null || utilizador.TipoUtilizadorId != 2)
            {
                return Forbid("Apenas administradores podem aceitar voluntários.");
            }

            var pedido = await _context.PedidosAjuda
                .Include(p => p.Voluntariados)
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

            return Ok("Voluntário aceite com sucesso e pedido atualizado para 'Em Progresso'.");
        }

        [HttpPost("{pedidoId}/rejeitar")]
        [Authorize]
        public async Task<IActionResult> RejeitarPedidoAjuda(int pedidoId)
        {
            // Obter o ID do utilizador autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            // Verificar se é administrador
            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null || utilizador.TipoUtilizadorId != 2)
            {
                return Forbid("Apenas administradores podem validar pedidos de ajuda.");
            }

            // Obter o pedido
            var pedido = await _context.PedidosAjuda.FindAsync(pedidoId);
            if (pedido == null)
            {
                return NotFound("Pedido de ajuda não encontrado.");
            }

            if (pedido.Estado != EstadoPedido.Pendente)
            {
                return BadRequest("Este pedido já foi validado ou está em progresso/concluído.");
            }

            // Atualizar o estado
            pedido.Estado = EstadoPedido.Rejeitado;

            await _context.SaveChangesAsync();

            return Ok("Pedido de ajuda validado com sucesso e colocado como 'Aberto'.");
        }

        [HttpPost("{pedidoId}/validar")]
        [Authorize]
        public async Task<IActionResult> ValidarPedidoAjuda(int pedidoId)
        {
            // Obter o ID do utilizador autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            // Verificar se é administrador
            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null || utilizador.TipoUtilizadorId != 2)
            {
                return Forbid("Apenas administradores podem validar pedidos de ajuda.");
            }

            // Obter o pedido
            var pedido = await _context.PedidosAjuda.FindAsync(pedidoId);
            if (pedido == null)
            {
                return NotFound("Pedido de ajuda não encontrado.");
            }

            if (pedido.Estado != EstadoPedido.Pendente)
            {
                return BadRequest("Este pedido já foi validado ou está em progresso/concluído.");
            }

            // Atualizar o estado
            pedido.Estado = EstadoPedido.Aberto;

            await _context.SaveChangesAsync();

            return Ok("Pedido de ajuda validado com sucesso e colocado como 'Aberto'.");
        }

        [HttpPost("validar-conclusao/{pedidoId}")]
        [Authorize]
        public async Task<IActionResult> ValidarConclusaoPedidoAjuda(int pedidoId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null || utilizador.TipoUtilizadorId != 2)
            {
                return Forbid("Apenas administradores podem validar pedidos de ajuda.");
            }

            var pedido = await _context.PedidosAjuda
                .Include(p => p.Utilizador) 
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

            if (pedido == null)
            {
                return NotFound("Pedido de ajuda não encontrado.");
            }

            if (pedido.Estado != EstadoPedido.EmProgresso)
            {
                return BadRequest("O pedido não está em progresso ou já foi concluído.");
            }

            var recetor = pedido.Utilizador;

            if (recetor == null)
            {
                return BadRequest("Não foi possível determinar o recetor do pedido.");
            }

            int recompensa = pedido.RecompensaCares ?? 0;

            recetor.NumCares += recompensa;

            var transacao = new Transacao
            {
                DataTransacao = DateTime.UtcNow,
                Quantidade = recompensa
            };

            var transacaoAjuda = new TransacaoAjuda
            {
                RecetorTran = recetor.UtilizadorId,
                Transacao = transacao,
                PedidoAjuda = new List<PedidoAjuda> { pedido }
            };

            pedido.Estado = EstadoPedido.Concluido;

            _context.Transacoes.Add(transacao);
            _context.TransacaoAjuda.Add(transacaoAjuda);

            await _context.SaveChangesAsync();

            return Ok("Pedido de ajuda concluído com sucesso. Recompensa atribuída e transação registada.");
        }


        #endregion


    }
}
