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
    public class TipoContactosController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public TipoContactosController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a lista de todos os tipos de contato disponíveis.
        /// </summary>
        /// <returns>Retorna uma lista de tipos de contato ou 500 Internal Server Error em caso de falha.</returns>

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoContacto>>> GetTipoContactos()
        {
            return await _context.TipoContactos.ToListAsync();
        }

        /// <summary>
        /// Obtém os detalhes de um tipo de contato específico, baseado no seu ID.
        /// </summary>
        /// <param name="id">ID do tipo de contato a ser obtido.</param>
        /// <returns>Retorna o tipo de contato correspondente ao ID ou 404 Not Found se o tipo de contato não existir.</returns>

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoContacto>> GetTipoContacto(int id)
        {
            var tipoContacto = await _context.TipoContactos.FindAsync(id);

            if (tipoContacto == null)
            {
                return NotFound();
            }

            return tipoContacto;
        }

        /// <summary>
        /// Atualiza os dados de um tipo de contato existente.
        /// </summary>
        /// <param name="id">ID do tipo de contato a ser atualizado.</param>
        /// <param name="tipoContacto">Objeto contendo os dados atualizados do tipo de contato.</param>
        /// <returns>Retorna 204 No Content se a atualização for bem-sucedida; retorna 400 Bad Request se os dados não coincidirem; 404 Not Found se o tipo de contato não existir ou 500 Internal Server Error em caso de falha.</returns>

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoContacto(int id, TipoContacto tipoContacto)
        {
            if (id != tipoContacto.TipoContactoId)
            {
                return BadRequest();
            }

            _context.Entry(tipoContacto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoContactoExists(id))
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
        /// Cria um novo tipo de contato.
        /// </summary>
        /// <param name="tipoContacto">Objeto contendo os dados do novo tipo de contato.</param>
        /// <returns>Retorna um status 201 Created com o tipo de contato criado, incluindo o URI do novo recurso; retorna 500 Internal Server Error em caso de falha.</returns>

        [HttpPost]
        public async Task<ActionResult<TipoContacto>> PostTipoContacto(TipoContacto tipoContacto)
        {
            _context.TipoContactos.Add(tipoContacto);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTipoContacto", new { id = tipoContacto.TipoContactoId }, tipoContacto);
        }

        /// <summary>
        /// Deleta um tipo de contato específico baseado no seu ID.
        /// </summary>
        /// <param name="id">ID do tipo de contato a ser deletado.</param>
        /// <returns>Retorna 204 No Content se o tipo de contato for deletado com sucesso; retorna 404 Not Found se o tipo de contato não existir ou 500 Internal Server Error em caso de falha.</returns>

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoContacto(int id)
        {
            var tipoContacto = await _context.TipoContactos.FindAsync(id);
            if (tipoContacto == null)
            {
                return NotFound();
            }

            _context.TipoContactos.Remove(tipoContacto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Verifica se existe um tipo de contato com o identificador especificado.
        /// </summary>
        /// <param name="id">ID do tipo de contato a verificar.</param>
        /// <returns>Retorna true se o tipo de contato existir; retorna false caso contrário.</returns>

        private bool TipoContactoExists(int id)
        {
            return _context.TipoContactos.Any(e => e.TipoContactoId == id);
        }
    }
}
