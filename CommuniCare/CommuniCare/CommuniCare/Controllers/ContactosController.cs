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
    /// Controlador responsável pela gestão dos contactos na aplicação CommuniCare.
    /// Permite criar, obter, atualizar e eliminar contactos de utilizadores.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ContactosController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        /// <summary>
        /// Construtor do controlador de contactos.
        /// </summary>
        /// <param name="context">Contexto da base de dados CommuniCare utilizado para aceder aos contactos.</param>
        public ContactosController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a lista completa de todos os contactos existentes na base de dados.
        /// </summary>
        /// <returns>Uma lista de todos os contactos registados.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contacto>>> GetContactos()
        {
            return await _context.Contactos.ToListAsync();
        }

        /// <summary>
        /// Obtém um contacto específico com base no seu identificador.
        /// </summary>
        /// <param name="id">Identificador do contacto a ser obtido.</param>
        /// <returns>O contacto correspondente ou um código 404 se não for encontrado.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Contacto>> GetContacto(int id)
        {
            var contacto = await _context.Contactos.FindAsync(id);

            if (contacto == null)
            {
                return NotFound();
            }

            return contacto;
        }

        /// <summary>
        /// Atualiza os dados de um contacto existente.
        /// </summary>
        /// <param name="id">Identificador do contacto a ser atualizado.</param>
        /// <param name="contacto">Objeto contacto com os dados atualizados.</param>
        /// <returns>Código 204 se a atualização for bem-sucedida; 400 ou 404 caso contrário.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContacto(int id, Contacto contacto)
        {
            if (id != contacto.ContactoId)
            {
                return BadRequest();
            }

            _context.Entry(contacto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactoExists(id))
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
        /// Cria um novo contacto na base de dados.
        /// </summary>
        /// <param name="contacto">Objeto contacto a ser criado.</param>
        /// <returns>O contacto criado e a localização do recurso.</returns>
        [HttpPost]
        public async Task<ActionResult<Contacto>> PostContacto(Contacto contacto)
        {
            _context.Contactos.Add(contacto);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContacto", new { id = contacto.ContactoId }, contacto);
        }

        /// <summary>
        /// Elimina um contacto específico com base no seu identificador.
        /// </summary>
        /// <param name="id">Identificador do contacto a ser eliminado.</param>
        /// <returns>Código 204 se a eliminação for bem-sucedida; 404 se o contacto não for encontrado.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContacto(int id)
        {
            var contacto = await _context.Contactos.FindAsync(id);
            if (contacto == null)
            {
                return NotFound();
            }

            _context.Contactos.Remove(contacto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Verifica se existe um contacto com o identificador especificado.
        /// </summary>
        /// <param name="id">Identificador do contacto a verificar.</param>
        /// <returns>True se o contacto existir; False caso contrário.</returns>
        private bool ContactoExists(int id)
        {
            return _context.Contactos.Any(e => e.ContactoId == id);
        }
    }
}
