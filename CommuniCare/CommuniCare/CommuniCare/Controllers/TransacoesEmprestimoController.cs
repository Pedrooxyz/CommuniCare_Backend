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
    public class TransacoesEmprestimoController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public TransacoesEmprestimoController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a lista de todas as transações de empréstimo.
        /// </summary>
        /// <returns>Retorna uma lista de transações de empréstimo ou 500 Internal Server Error em caso de falha.</returns>

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransacaoEmprestimo>>> GetTransacaoEmprestimos()
        {
            return await _context.TransacoesEmprestimo.ToListAsync();
        }

        /// <summary>
        /// Obtém os detalhes de uma transação de empréstimo específica, baseada no seu ID.
        /// </summary>
        /// <param name="id">ID da transação de empréstimo a ser obtida.</param>
        /// <returns>Retorna a transação de empréstimo correspondente ao ID ou 404 Not Found se a transação não existir.</returns>

        //[HttpGet("{id}")]
        //public async Task<ActionResult<TransacaoEmprestimo>> GetTransacaoEmprestimo(int id)
        //{
        //    var transacaoEmprestimo = await _context.TransacoesEmprestimo.FindAsync(id);

        //    if (transacaoEmprestimo == null)
        //    {
        //        return NotFound();
        //    }

        //    return transacaoEmprestimo;
        //}

        /// <summary>
        /// Atualiza os dados de uma transação de empréstimo existente.
        /// </summary>
        /// <param name="id">ID da transação de empréstimo a ser atualizada.</param>
        /// <param name="transacaoEmprestimo">Objeto contendo os dados atualizados da transação de empréstimo.</param>
        /// <returns>Retorna 204 No Content se a atualização for bem-sucedida; retorna 400 Bad Request se os dados não coincidirem; 404 Not Found se a transação de empréstimo não existir ou 500 Internal Server Error em caso de falha.</returns>

        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutTransacaoEmprestimo(int id, TransacaoEmprestimo transacaoEmprestimo)
        //{
        //    if (id != transacaoEmprestimo.TransacaoId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(transacaoEmprestimo).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!TransacaoEmprestimoExists(id))
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

        /// <summary>
        /// Cria uma nova transação de empréstimo.
        /// </summary>
        /// <param name="transacaoEmprestimo">Objeto contendo os dados da nova transação de empréstimo.</param>
        /// <returns>Retorna um status 201 Created com a transação de empréstimo criada, incluindo o URI do novo recurso; retorna 500 Internal Server Error em caso de falha.</returns>

        //[HttpPost]
        //public async Task<ActionResult<TransacaoEmprestimo>> PostTransacaoEmprestimo(TransacaoEmprestimo transacaoEmprestimo)
        //{
        //    _context.TransacoesEmprestimo.Add(transacaoEmprestimo);
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        if (TransacaoEmprestimoExists(transacaoEmprestimo.TransacaoId))
        //        {
        //            return Conflict();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return CreatedAtAction("GetTransacaoEmprestimo", new { id = transacaoEmprestimo.TransacaoId }, transacaoEmprestimo);
        //}

        /// <summary>
        /// Deleta uma transação de empréstimo específica baseada no seu ID.
        /// </summary>
        /// <param name="id">ID da transação de empréstimo a ser deletada.</param>
        /// <returns>Retorna 204 No Content se a transação de empréstimo for deletada com sucesso; retorna 404 Not Found se a transação de empréstimo não existir ou 500 Internal Server Error em caso de falha.</returns>

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteTransacaoEmprestimo(int id)
        //{
        //    var transacaoEmprestimo = await _context.TransacoesEmprestimo.FindAsync(id);
        //    if (transacaoEmprestimo == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.TransacoesEmprestimo.Remove(transacaoEmprestimo);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        /// <summary>
        /// Verifica se existe uma transação de empréstimo com o identificador especificado.
        /// </summary>
        /// <param name="id">ID da transação de empréstimo a verificar.</param>
        /// <returns>Retorna true se a transação de empréstimo existir; retorna false caso contrário.</returns>

        private bool TransacaoEmprestimoExists(int id)
        {
            return _context.TransacoesEmprestimo.Any(e => e.TransacaoId == id);
        }
    }
}
