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

        /// <summary>
        /// Obtém todos os voluntariados registados no sistema.
        /// </summary>
        /// <returns>Lista de voluntariados existentes.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Voluntariado>>> GetVoluntariados()
        {
            return await _context.Voluntariados.ToListAsync();
        }


        #region CONTROLLERS AUTOMÁTICOS
        ///// <summary>
        ///// Obtém os detalhes de um voluntariado específico com base no identificador fornecido.
        ///// </summary>
        ///// <param name="id">Identificador do voluntariado a ser recuperado.</param>
        ///// <returns>Detalhes do voluntariado, ou NotFound se o voluntariado não for encontrado.</returns>
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Voluntariado>> GetVoluntariado(int id)
        //{
        //    var voluntariado = await _context.Voluntariados.FindAsync(id);

        //    if (voluntariado == null)
        //    {
        //        return NotFound();
        //    }

        //    return voluntariado;
        //}

        ///// <summary>
        ///// Atualiza os dados de um voluntariado existente.
        ///// </summary>
        ///// <param name="id">Identificador do voluntariado a ser atualizado.</param>
        ///// <param name="voluntariado">Objeto contendo os novos dados do voluntariado.</param>
        ///// <returns>Resultado da atualização. Retorna NoContent se bem-sucedido, ou NotFound se o voluntariado não for encontrado.</returns>
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutVoluntariado(int id, Voluntariado voluntariado)
        //{
        //    if (id != voluntariado.PedidoId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(voluntariado).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!VoluntariadoExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        ///// <summary>
        ///// Cria um novo voluntariado no sistema.
        ///// </summary>
        ///// <param name="voluntariado">Objeto contendo os dados do voluntariado a ser adicionado.</param>
        ///// <returns>Voluntariado criado, ou Conflict se o voluntariado já existir.</returns>
        //[HttpPost]
        //public async Task<ActionResult<Voluntariado>> PostVoluntariado(Voluntariado voluntariado)
        //{
        //    _context.Voluntariados.Add(voluntariado);
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        if (VoluntariadoExists(voluntariado.PedidoId))
        //        {
        //            return Conflict();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return CreatedAtAction("GetVoluntariado", new { id = voluntariado.PedidoId }, voluntariado);
        //}

        ///// <summary>
        ///// Remove um voluntariado do sistema com base no identificador fornecido.
        ///// </summary>
        ///// <param name="id">Identificador do voluntariado a ser removido.</param>
        ///// <returns>Resultado da remoção. Retorna NoContent se bem-sucedido, ou NotFound se o voluntariado não for encontrado.</returns>
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteVoluntariado(int id)
        //{
        //    var voluntariado = await _context.Voluntariados.FindAsync(id);
        //    if (voluntariado == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Voluntariados.Remove(voluntariado);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        #endregion
        private bool VoluntariadoExists(int id)
        {
            return _context.Voluntariados.Any(e => e.PedidoId == id);
        }

        /// <summary>
        /// Rejeita um voluntário para um pedido específico e envia uma notificação.
        /// Apenas administradores têm permissão para rejeitar voluntários.
        /// </summary>
        /// <param name="pedidoId">Identificador do pedido.</param>
        /// <param name="utilizadorId">Identificador do voluntário a ser rejeitado.</param>
        /// <returns>Resultado da rejeição. Retorna Ok se bem-sucedido, Unauthorized se não autenticado, ou Forbid se não for um administrador.</returns>
        [HttpPost("pedidos/{pedidoId}/voluntarios/{utilizadorId}/rejeitar")]
        [Authorize]
        public async Task<IActionResult> RejeitarVoluntario(int pedidoId, int utilizadorId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int adminId = int.Parse(userIdClaim.Value);

            // Verifica se o usuário é um administrador
            var admin = await _context.Utilizadores.FindAsync(adminId);
            if (admin == null || admin.TipoUtilizadorId != 2)
            {
                return Forbid("Apenas administradores podem rejeitar voluntários.");
            }

            // Encontra o voluntariado a ser rejeitado, que esteja no estado Pendente
            var voluntariado = await _context.Voluntariados
                .Include(v => v.Pedido)
                .Include(v => v.Utilizador)
                .FirstOrDefaultAsync(v =>
                    v.PedidoId == pedidoId &&
                    v.UtilizadorId == utilizadorId &&
                    v.Estado == EstadoVoluntariado.Pendente);

            if (voluntariado == null)
            {
                return NotFound("Voluntariado não encontrado ou já foi processado.");
            }

            // Remove o voluntariado (rejeita o voluntário)
            _context.Voluntariados.Remove(voluntariado);

            // Cria uma notificação para o voluntário
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

            // Cria uma notificação para o requisitante
            var notificacaoRequisitante = new Notificacao
            {
                Mensagem = $"Um voluntário foi rejeitado para o teu pedido #{voluntariado.Pedido.PedidoId}.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                PedidoId = voluntariado.PedidoId,
                UtilizadorId = voluntariado.Pedido.UtilizadorId,
                ItemId = null
            };

            _context.Notificacaos.Add(notificacaoRequisitante);

            // Salva as alterações no banco de dados
            await _context.SaveChangesAsync();

            return Ok("Voluntário rejeitado com sucesso.");
        }


        /// <summary>
        /// Aceita um voluntário para um pedido específico e atualiza o estado do pedido caso o número de voluntários aceites seja atingido.
        /// Apenas administradores têm permissão para aceitar voluntários.
        /// </summary>
        /// <param name="pedidoId">Identificador do pedido.</param>
        /// <param name="utilizadorId">Identificador do voluntário a ser aceite.</param>
        /// <returns>Resultado da aceitação. Retorna Ok se bem-sucedido, Unauthorized se não autenticado, ou Forbid se não for um administrador.</returns>
        [HttpPost("pedidos/{pedidoId}/voluntarios/{utilizadorId}/aceitar")]
        [Authorize]
        public async Task<IActionResult> AceitarVoluntario(int pedidoId, int utilizadorId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int adminId = int.Parse(userIdClaim.Value);

            var utilizador = await _context.Utilizadores.FindAsync(adminId);
            if (utilizador == null || utilizador.TipoUtilizadorId != 2)
            {
                return Forbid("Apenas administradores podem aceitar voluntários.");
            }

            var voluntariado = await _context.Voluntariados
                .Include(v => v.Pedido)
                .Include(v => v.Pedido.Voluntariados)
                .Include(v => v.Pedido.Utilizador)
                .FirstOrDefaultAsync(v =>
                    v.PedidoId == pedidoId &&
                    v.UtilizadorId == utilizadorId &&
                    v.Estado == EstadoVoluntariado.Pendente);

            if (voluntariado == null)
            {
                return BadRequest("Voluntariado não encontrado, dados incorretos ou não está pendente.");
            }

            voluntariado.Estado = EstadoVoluntariado.Aceite;

            var pedido = voluntariado.Pedido;

            int voluntariosAceites = pedido.Voluntariados.Count(v => v.Estado == EstadoVoluntariado.Aceite);
            if (pedido.NPessoas.HasValue && voluntariosAceites >= pedido.NPessoas.Value)
            {
                pedido.Estado = EstadoPedido.EmProgresso;
            }

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