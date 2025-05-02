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
    public class NotificacoesController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public NotificacoesController(CommuniCareContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Obtém a lista de todas as notificações no sistema.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="Notificacao"/> representando todas as notificações.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notificacao>>> GetNotificacaos()
        {
            return await _context.Notificacaos.ToListAsync();
        }

        #region CONTROLLERS AUTOMÁTICOS
        ///// <summary>
        ///// Obtém os detalhes de uma notificação específica com base no identificador.
        ///// </summary>
        ///// <param name="id">Identificador único da notificação.</param>
        ///// <returns>Um objeto <see cref="Notificacao"/> com os dados da notificação especificada, ou NotFound se a notificação não existir.</returns>
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Notificacao>> GetNotificacao(int id)
        //{
        //    var notificacao = await _context.Notificacaos.FindAsync(id);

        //    if (notificacao == null)
        //    {
        //        return NotFound();
        //    }

        //    return notificacao;
        //}

        ///// <summary>
        ///// Atualiza os dados de uma notificação existente.
        ///// </summary>
        ///// <param name="id">Identificador da notificação a ser atualizada.</param>
        ///// <param name="notificacao">Objeto <see cref="Notificacao"/> contendo os dados atualizados da notificação.</param>
        ///// <returns>Um status de resposta que indica o sucesso ou falha da operação.</returns>
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutNotificacao(int id, Notificacao notificacao)
        //{
        //    if (id != notificacao.NotificacaoId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(notificacao).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!NotificacaoExists(id))
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
        ///// Cria uma nova notificação no sistema.
        ///// </summary>
        ///// <param name="notificacao">Objeto <see cref="Notificacao"/> com os dados da notificação a ser criada.</param>
        ///// <returns>O objeto <see cref="Notificacao"/> criado, incluindo o identificador da notificação.</returns>
        //[HttpPost]
        //public async Task<ActionResult<Notificacao>> PostNotificacao(Notificacao notificacao)
        //{
        //    _context.Notificacaos.Add(notificacao);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetNotificacao", new { id = notificacao.NotificacaoId }, notificacao);
        //}

        ///// <summary>
        ///// Exclui uma notificação do sistema com base no identificador.
        ///// </summary>
        ///// <param name="id">Identificador único da notificação a ser excluída.</param>
        ///// <returns>Um status de resposta que indica o sucesso ou falha da operação.</returns>
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteNotificacao(int id)
        //{
        //    var notificacao = await _context.Notificacaos.FindAsync(id);
        //    if (notificacao == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Notificacaos.Remove(notificacao);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        #endregion

        /// <summary>
        /// Verifica se existe uma notificação com o identificador especificado.
        /// </summary>
        /// <param name="id">Identificador da notificação a verificar.</param>
        /// <returns>True se a notificação existir; False caso contrário.</returns>
        private bool NotificacaoExists(int id)
        {
            return _context.Notificacaos.Any(e => e.NotificacaoId == id);
        }

        /// <summary>
        /// Obtém as notificações do utilizador autenticado.
        /// </summary>
        /// <returns>Uma lista de notificações não lidas do utilizador autenticado, ou NotFound se não houver notificações.</returns>
        /// <response code="401">Se o utilizador não estiver autenticado.</response>
        /// <response code="404">Se não houver notificações para o utilizador.</response>
        [HttpGet("Notificacoes")]
        [Authorize]
        public async Task<IActionResult> VerNotificacoes()
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);


            var notificacoes = await _context.Notificacaos
                .Where(n => n.UtilizadorId == utilizadorId)
                .OrderByDescending(n => n.DataMensagem)
                .ToListAsync();

            if (notificacoes == null || !notificacoes.Any())
            {
                return NotFound("Não há notificações para mostrar.");
            }

            return Ok(notificacoes);
        }

    }
}
