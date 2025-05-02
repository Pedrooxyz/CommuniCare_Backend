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
    /// Controlador responsável pela gestão de chats na aplicação CommuniCare.
    /// Permite criar, obter, atualizar e eliminar conversas de chat.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {


        private readonly CommuniCareContext _context;

        /// <summary>
        /// Construtor do controlador de chats.
        /// </summary>
        /// <param name="context">Contexto da base de dados CommuniCare utilizado para aceder aos chats.</param>
        public ChatsController(CommuniCareContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Obtém a lista completa de todos os chats existentes na base de dados.
        /// </summary>
        /// <returns>Uma lista de todas as conversas de chat registadas.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Chat>>> GetChats()
        {
            return await _context.Chats.ToListAsync();
        }



        // <summary>
        /// Obtém um chat específico com base no seu identificador.
        /// </summary>
        /// <param name="id">Identificador do chat a ser obtido.</param>
        /// <returns>O chat correspondente ou um código 404 se não for encontrado.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Chat>> GetChat(int id)
        {
            var chat = await _context.Chats.FindAsync(id);

            if (chat == null)
            {
                return NotFound();
            }

            return chat;
        }


        /// <summary>
        /// Atualiza os dados de um chat existente.
        /// </summary>
        /// <param name="id">Identificador do chat a ser atualizado.</param>
        /// <param name="chat">Objeto chat com os dados atualizados.</param>
        /// <returns>Código 204 se a atualização for bem-sucedida; 400 ou 404 caso contrário.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChat(int id, Chat chat)
        {
            if (id != chat.ChatId)
            {
                return BadRequest();
            }

            _context.Entry(chat).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChatExists(id))
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
        /// Cria um novo chat na base de dados.
        /// </summary>
        /// <param name="chat">Objeto chat a ser criado.</param>
        /// <returns>O chat criado e a localização do recurso.</returns>
        [HttpPost]
        public async Task<ActionResult<Chat>> PostChat(Chat chat)
        {
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetChat", new { id = chat.ChatId }, chat);
        }

        /// <summary>
        /// Elimina um chat específico com base no seu identificador.
        /// </summary>
        /// <param name="id">Identificador do chat a ser eliminado.</param>
        /// <returns>Código 204 se a eliminação for bem-sucedida; 404 se o chat não for encontrado.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChat(int id)
        {
            var chat = await _context.Chats.FindAsync(id);
            if (chat == null)
            {
                return NotFound();
            }

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Verifica se existe um chat com o identificador especificado.
        /// </summary>
        /// <param name="id">Identificador do chat a verificar.</param>
        /// <returns>True se o chat existir; False caso contrário.</returns>
        private bool ChatExists(int id)
        {
            return _context.Chats.Any(e => e.ChatId == id);
        }
    }
}
