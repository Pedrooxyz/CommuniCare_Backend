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
    public class MensagensController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public MensagensController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a lista de todas as mensagens no sistema.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="Mensagem"/> representando todas as mensagens.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mensagem>>> GetMensagems()
        {
            return await _context.Mensagens.ToListAsync();
        }

        /// <summary>
        /// Obtém os detalhes de uma mensagem específica com base no identificador.
        /// </summary>
        /// <param name="id">Identificador único da mensagem.</param>
        /// <returns>Um objeto <see cref="Mensagem"/> com os dados da mensagem especificada, ou NotFound se a mensagem não existir.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Mensagem>> GetMensagem(int id)
        {
            var mensagem = await _context.Mensagens.FindAsync(id);

            if (mensagem == null)
            {
                return NotFound();
            }

            return mensagem;
        }

        /// <summary>
        /// Atualiza os dados de uma mensagem existente.
        /// </summary>
        /// <param name="id">Identificador da mensagem a ser atualizada.</param>
        /// <param name="mensagem">Objeto <see cref="Mensagem"/> contendo os dados atualizados da mensagem.</param>
        /// <returns>Um status de resposta que indica o sucesso ou falha da operação.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMensagem(int id, Mensagem mensagem)
        {
            if (id != mensagem.MensagemId)
            {
                return BadRequest();
            }

            _context.Entry(mensagem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MensagemExists(id))
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

        /// <summary>
        /// Cria uma nova mensagem no sistema.
        /// </summary>
        /// <param name="mensagem">Objeto <see cref="Mensagem"/> com os dados da mensagem a ser criada.</param>
        /// <returns>O objeto <see cref="Mensagem"/> criado, incluindo o identificador da mensagem.</returns>
        [HttpPost]
        public async Task<ActionResult<Mensagem>> PostMensagem(Mensagem mensagem)
        {
            _context.Mensagens.Add(mensagem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMensagem", new { id = mensagem.MensagemId }, mensagem);
        }

        /// <summary>
        /// Exclui uma mensagem do sistema com base no identificador.
        /// </summary>
        /// <param name="id">Identificador único da mensagem a ser excluída.</param>
        /// <returns>Um status de resposta que indica o sucesso ou falha da operação.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMensagem(int id)
        {
            var mensagem = await _context.Mensagens.FindAsync(id);
            if (mensagem == null)
            {
                return NotFound();
            }

            _context.Mensagens.Remove(mensagem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Verifica se existe uma mensagem com o identificador especificado.
        /// </summary>
        /// <param name="id">Identificador do chat a verificar.</param>
        /// <returns>True se a mensagem existir; False caso contrário.</returns>
        private bool MensagemExists(int id)
        {
            return _context.Mensagens.Any(e => e.MensagemId == id);
        }
    }
}
