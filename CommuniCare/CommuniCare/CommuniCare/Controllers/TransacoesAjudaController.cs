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
    public class TransacoesAjudaController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public TransacoesAjudaController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a lista de todas as transações de ajuda.
        /// </summary>
        /// <returns>Retorna uma lista de transações de ajuda ou 500 Internal Server Error em caso de falha.</returns>

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransacaoAjuda>>> GetTransacaoAjuda()
        {
            return await _context.TransacaoAjuda.ToListAsync();
        }

        /// <summary>
        /// Obtém os detalhes de uma transação de ajuda específica, baseada no seu ID.
        /// </summary>
        /// <param name="id">ID da transação de ajuda a ser obtida.</param>
        /// <returns>Retorna a transação de ajuda correspondente ao ID ou 404 Not Found se a transação de ajuda não existir.</returns>

        [HttpGet("{id}")]
        public async Task<ActionResult<TransacaoAjuda>> GetTransacaoAjuda(int id)
        {
            var transacaoAjuda = await _context.TransacaoAjuda.FindAsync(id);

            if (transacaoAjuda == null)
            {
                return NotFound();
            }

            return transacaoAjuda;
        }

        /// <summary>
        /// Atualiza os dados de uma transação de ajuda existente.
        /// </summary>
        /// <param name="id">ID da transação de ajuda a ser atualizada.</param>
        /// <param name="transacaoAjuda">Objeto contendo os dados atualizados da transação de ajuda.</param>
        /// <returns>Retorna 204 No Content se a atualização for bem-sucedida; retorna 400 Bad Request se os dados não coincidirem; 404 Not Found se a transação de ajuda não existir ou 500 Internal Server Error em caso de falha.</returns>

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransacaoAjuda(int id, TransacaoAjuda transacaoAjuda)
        {
            if (id != transacaoAjuda.TransacaoId)
            {
                return BadRequest();
            }

            _context.Entry(transacaoAjuda).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransacaoAjudaExists(id))
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
        /// Cria uma nova transação de ajuda.
        /// </summary>
        /// <param name="transacaoAjuda">Objeto contendo os dados da nova transação de ajuda.</param>
        /// <returns>Retorna um status 201 Created com a transação de ajuda criada, incluindo o URI do novo recurso; retorna 500 Internal Server Error em caso de falha.</returns>

        [HttpPost]
        public async Task<ActionResult<TransacaoAjuda>> PostTransacaoAjuda(TransacaoAjuda transacaoAjuda)
        {
            _context.TransacaoAjuda.Add(transacaoAjuda);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TransacaoAjudaExists(transacaoAjuda.TransacaoId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTransacaoAjuda", new { id = transacaoAjuda.TransacaoId }, transacaoAjuda);
        }

        /// <summary>
        /// Verifica se existe uma transação de ajuda com o identificador especificado.
        /// </summary>
        /// <param name="id">ID da transação de ajuda a verificar.</param>
        /// <returns>Retorna true se a transação de ajuda existir; retorna false caso contrário.</returns>

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransacaoAjuda(int id)
        {
            var transacaoAjuda = await _context.TransacaoAjuda.FindAsync(id);
            if (transacaoAjuda == null)
            {
                return NotFound();
            }

            _context.TransacaoAjuda.Remove(transacaoAjuda);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransacaoAjudaExists(int id)
        {
            return _context.TransacaoAjuda.Any(e => e.TransacaoId == id);
        }
    }
}
