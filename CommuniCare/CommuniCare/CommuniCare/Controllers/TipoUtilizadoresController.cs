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
    public class TipoUtilizadoresController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public TipoUtilizadoresController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a lista de todos os tipos de utilizadores disponíveis.
        /// </summary>
        /// <returns>Retorna uma lista de tipos de utilizadores ou 500 Internal Server Error em caso de falha.</returns>

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoUtilizador>>> GetTipoUtilizadors()
        {
            return await _context.TipoUtilizadors.ToListAsync();
        }

        /// <summary>
        /// Obtém os detalhes de um tipo de utilizador específico, baseado no seu ID.
        /// </summary>
        /// <param name="id">ID do tipo de utilizador a ser obtido.</param>
        /// <returns>Retorna o tipo de utilizador correspondente ao ID ou 404 Not Found se o tipo de utilizador não existir.</returns>

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoUtilizador>> GetTipoUtilizador(int id)
        {
            var tipoUtilizador = await _context.TipoUtilizadors.FindAsync(id);

            if (tipoUtilizador == null)
            {
                return NotFound();
            }

            return tipoUtilizador;
        }

        /// <summary>
        /// Atualiza os dados de um tipo de utilizador existente.
        /// </summary>
        /// <param name="id">ID do tipo de utilizador a ser atualizado.</param>
        /// <param name="tipoUtilizador">Objeto contendo os dados atualizados do tipo de utilizador.</param>
        /// <returns>Retorna 204 No Content se a atualização for bem-sucedida; retorna 400 Bad Request se os dados não coincidirem; 404 Not Found se o tipo de utilizador não existir ou 500 Internal Server Error em caso de falha.</returns>

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoUtilizador(int id, TipoUtilizador tipoUtilizador)
        {
            if (id != tipoUtilizador.TipoUtilizadorId)
            {
                return BadRequest();
            }

            _context.Entry(tipoUtilizador).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoUtilizadorExists(id))
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
        /// Cria um novo tipo de utilizador.
        /// </summary>
        /// <param name="tipoUtilizador">Objeto contendo os dados do novo tipo de utilizador.</param>
        /// <returns>Retorna um status 201 Created com o tipo de utilizador criado, incluindo o URI do novo recurso; retorna 500 Internal Server Error em caso de falha.</returns>

        [HttpPost]
        public async Task<ActionResult<TipoUtilizador>> PostTipoUtilizador(TipoUtilizador tipoUtilizador)
        {
            _context.TipoUtilizadors.Add(tipoUtilizador);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTipoUtilizador", new { id = tipoUtilizador.TipoUtilizadorId }, tipoUtilizador);
        }

        /// <summary>
        /// Deleta um tipo de utilizador específico baseado no seu ID.
        /// </summary>
        /// <param name="id">ID do tipo de utilizador a ser deletado.</param>
        /// <returns>Retorna 204 No Content se o tipo de utilizador for deletado com sucesso; retorna 404 Not Found se o tipo de utilizador não existir ou 500 Internal Server Error em caso de falha.</returns>

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoUtilizador(int id)
        {
            var tipoUtilizador = await _context.TipoUtilizadors.FindAsync(id);
            if (tipoUtilizador == null)
            {
                return NotFound();
            }

            _context.TipoUtilizadors.Remove(tipoUtilizador);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Verifica se existe um tipo de utilizador com o identificador especificado.
        /// </summary>
        /// <param name="id">ID do tipo de utilizador a verificar.</param>
        /// <returns>Retorna true se o tipo de utilizador existir; retorna false caso contrário.</returns>

        private bool TipoUtilizadorExists(int id)
        {
            return _context.TipoUtilizadors.Any(e => e.TipoUtilizadorId == id);
        }
    }
}
