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
    public class MoradasController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public MoradasController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a lista de todas as moradas no sistema.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="Morada"/> representando todas as moradas.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Morada>>> GetMorada()
        {
            return await _context.Morada.ToListAsync();
        }

        /// <summary>
        /// Obtém os detalhes de uma morada específica com base no identificador.
        /// </summary>
        /// <param name="id">Identificador único da morada.</param>
        /// <returns>Um objeto <see cref="Morada"/> com os dados da morada especificada, ou NotFound se a morada não existir.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Morada>> GetMorada(int id)
        {
            var morada = await _context.Morada.FindAsync(id);

            if (morada == null)
            {
                return NotFound();
            }

            return morada;
        }

        /// <summary>
        /// Atualiza os dados de uma morada existente.
        /// </summary>
        /// <param name="id">Identificador da morada a ser atualizada.</param>
        /// <param name="morada">Objeto <see cref="Morada"/> contendo os dados atualizados da morada.</param>
        /// <returns>Um status de resposta que indica o sucesso ou falha da operação.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMorada(int id, Morada morada)
        {
            if (id != morada.MoradaId)
            {
                return BadRequest();
            }

            _context.Entry(morada).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MoradaExists(id))
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
        /// Cria uma nova morada no sistema.
        /// </summary>
        /// <param name="morada">Objeto <see cref="Morada"/> com os dados da morada a ser criada.</param>
        /// <returns>O objeto <see cref="Morada"/> criado, incluindo o identificador da morada.</returns>
        [HttpPost]
        public async Task<ActionResult<Morada>> PostMorada(Morada morada)
        {
            _context.Morada.Add(morada);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMorada", new { id = morada.MoradaId }, morada);
        }

        /// <summary>
        /// Exclui uma morada do sistema com base no identificador.
        /// </summary>
        /// <param name="id">Identificador único da morada a ser excluída.</param>
        /// <returns>Um status de resposta que indica o sucesso ou falha da operação.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMorada(int id)
        {
            var morada = await _context.Morada.FindAsync(id);
            if (morada == null)
            {
                return NotFound();
            }

            _context.Morada.Remove(morada);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Verifica se existe uma morada com o identificador especificado.
        /// </summary>
        /// <param name="id">Identificador da morada a verificar.</param>
        /// <returns>True se a morada existir; False caso contrário.</returns>
        private bool MoradaExists(int id)
        {
            return _context.Morada.Any(e => e.MoradaId == id);
        }
    }
}
