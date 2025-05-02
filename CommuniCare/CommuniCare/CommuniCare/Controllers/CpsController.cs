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
    /// <summary>
    /// Controlador responsável pela gestão dos códigos postais (CPs) na aplicação CommuniCare.
    /// Permite criar, obter, atualizar e eliminar códigos postais armazenados na base de dados.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CpsController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        /// <summary>
        /// Construtor do controlador de códigos postais.
        /// </summary>
        /// <param name="context">Contexto da base de dados CommuniCare utilizado para aceder aos códigos postais.</param>
        public CpsController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a lista completa de todos os códigos postais existentes na base de dados.
        /// </summary>
        /// <returns>Uma lista de todos os códigos postais registados.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cp>>> GetCps()
        {
            return await _context.Cps.ToListAsync();
        }

        /// <summary>
        /// Obtém um código postal específico com base no seu identificador.
        /// </summary>
        /// <param name="id">Código postal a ser obtido.</param>
        /// <returns>O código postal correspondente ou um código 404 se não for encontrado.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Cp>> GetCp(int id)
        {
            var cp = await _context.Cps.FindAsync(id);

            if (cp == null)
            {
                return NotFound();
            }

            return cp;
        }

        /// <summary>
        /// Atualiza os dados de um código postal existente.
        /// </summary>
        /// <param name="id">Código postal a ser atualizado.</param>
        /// <param name="cp">Objeto Cp com os dados atualizados.</param>
        /// <returns>Código 204 se a atualização for bem-sucedida; 400 ou 404 caso contrário.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCp(string id, Cp cp)
        {
            if (id != cp.CPostal)
            {
                return BadRequest();
            }

            _context.Entry(cp).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CpExists(id))
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
        /// Cria um novo código postal na base de dados.
        /// </summary>
        /// <param name="cp">Objeto Cp a ser criado.</param>
        /// <returns>O código postal criado e a localização do recurso.</returns>
        [HttpPost]
        public async Task<ActionResult<Cp>> PostCp(Cp cp)
        {
            _context.Cps.Add(cp);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCp", new { id = cp.CPostal }, cp);
        }




        /// <summary>
        /// Elimina um código postal específico com base no seu identificador.
        /// </summary>
        /// <param name="id">Código postal a ser eliminado.</param>
        /// <returns>Código 204 se a eliminação for bem-sucedida; 404 se o código postal não for encontrado.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCp(int id)
        {
            var cp = await _context.Cps.FindAsync(id);
            if (cp == null)
            {
                return NotFound();
            }

            _context.Cps.Remove(cp);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Verifica se existe um código postal com o identificador especificado.
        /// </summary>
        /// <param name="id">Código postal a verificar.</param>
        /// <returns>True se o código postal existir; False caso contrário.</returns>
        /// 
        private bool CpExists(string id)
        {
            return _context.Cps.Any(e => e.CPostal == id);
        }
    }
}
