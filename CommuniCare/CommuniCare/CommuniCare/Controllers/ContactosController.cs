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

        /// <summary>
        /// Obtém os contactos do utilizador autenticado.
        /// </summary>
        /// <returns>Retorna a lista de contactos do utilizador autenticado ou 401 Unauthorized se o utilizador não for autenticado.</returns>
        [HttpGet("ContactosUtilizador")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ContactoDTO>>> GetContactosUtilizador()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (idClaim is null) return Unauthorized();

            if (!int.TryParse(idClaim.Value, out var userId))
                return Unauthorized();

            var contactos = await _context.Contactos
                .AsNoTracking()
                .Where(c => c.UtilizadorId == userId)
                .Select(c => new ContactoDTO
                {
                    TipoContactoId = c.TipoContactoId,
                    NumContacto = c.NumContacto
                })
                .ToListAsync();

            return Ok(contactos);
        }

        /// <summary>
        /// Adiciona um novo contacto para o utilizador autenticado.
        /// </summary>
        /// <param name="novoContacto">Os dados do novo contacto a ser adicionado.</param>
        /// <returns>Retorna o contacto adicionado ou 401 Unauthorized se o utilizador não for autenticado.</returns>
        [HttpPost("AdicionarContacto")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ContactoDTO>> AdicionarContacto([FromBody] ContactoDTO novoContacto)
        {

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (idClaim is null) return Unauthorized();

            if (!int.TryParse(idClaim.Value, out var userId))
                return Unauthorized();


            var contacto = new Contacto
            {
                UtilizadorId = userId,
                TipoContactoId = novoContacto.TipoContactoId,
                NumContacto = novoContacto.NumContacto
            };


            _context.Contactos.Add(contacto);
            await _context.SaveChangesAsync();


            var contactoAdicionado = new ContactoDTO
            {
                TipoContactoId = contacto.TipoContactoId,
                NumContacto = contacto.NumContacto
            };

            return Ok(contactoAdicionado);
        }

        [Authorize]
        [HttpGet("ItensUtilizador/{id}")]
        public async Task<ActionResult<IEnumerable<ItemEmprestimo>>> GetItensDeOutroUtilizador(int id)
        {

            var existeUtilizador = await _context.Utilizadores
                .AsNoTracking()
                .AnyAsync(u => u.UtilizadorId == id);

            if (!existeUtilizador)
                return NotFound($"Utilizador com ID {id} não encontrado.");

            var itemIds = await _context.ItemEmprestimoUtilizadores
                .Where(rel => rel.UtilizadorId == id && rel.TipoRelacao == "Dono")
                .Select(rel => rel.ItemId)
                .ToListAsync();

            var itens = await _context.ItensEmprestimo
                .Where(item => itemIds.Contains(item.ItemId) &&
                               item.Disponivel != EstadoItemEmprestimo.IndisponivelPermanentemente)
                .ToListAsync();

            return Ok(itens);
        }
    }
}
