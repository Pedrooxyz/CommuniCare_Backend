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
    public class TransacoesController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public TransacoesController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a lista de todas as transações.
        /// </summary>
        /// <returns>Retorna uma lista de transações ou 500 Internal Server Error em caso de falha.</returns>

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transacao>>> GetTransacaos()
        {
            return await _context.Transacoes.ToListAsync();
        }

        /// <summary>
        /// Obtém os detalhes de uma transação específica, baseada no seu ID.
        /// </summary>
        /// <param name="id">ID da transação a ser obtida.</param>
        /// <returns>Retorna a transação correspondente ao ID ou 404 Not Found se a transação não existir.</returns>

        //[HttpGet("{id}")]
        //public async Task<ActionResult<Transacao>> GetTransacao(int id)
        //{
        //    var transacao = await _context.Transacoes.FindAsync(id);

        //    if (transacao == null)
        //    {
        //        return NotFound();
        //    }

        //    return transacao;
        //}

        /// <summary>
        /// Atualiza os dados de uma transação existente.
        /// </summary>
        /// <param name="id">ID da transação a ser atualizada.</param>
        /// <param name="transacao">Objeto contendo os dados atualizados da transação.</param>
        /// <returns>Retorna 204 No Content se a atualização for bem-sucedida; retorna 400 Bad Request se os dados não coincidirem; 404 Not Found se a transação não existir ou 500 Internal Server Error em caso de falha.</returns>

        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutTransacao(int id, Transacao transacao)
        //{
        //    if (id != transacao.TransacaoId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(transacao).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!TransacaoExists(id))
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
        /// Cria uma nova transação.
        /// </summary>
        /// <param name="transacao">Objeto contendo os dados da nova transação.</param>
        /// <returns>Retorna um status 201 Created com a transação criada, incluindo o URI do novo recurso; retorna 500 Internal Server Error em caso de falha.</returns>

        //[HttpPost]
        //public async Task<ActionResult<Transacao>> PostTransacao(Transacao transacao)
        //{
        //    _context.Transacoes.Add(transacao);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetTransacao", new { id = transacao.TransacaoId }, transacao);
        //}

        /// <summary>
        /// Deleta uma transação específica baseada no seu ID.
        /// </summary>
        /// <param name="id">ID da transação a ser deletada.</param>
        /// <returns>Retorna 204 No Content se a transação for deletada com sucesso; retorna 404 Not Found se a transação não existir ou 500 Internal Server Error em caso de falha.</returns>

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteTransacao(int id)
        //{
        //    var transacao = await _context.Transacoes.FindAsync(id);
        //    if (transacao == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Transacoes.Remove(transacao);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        /// <summary>
        /// Verifica se existe uma transação com o identificador especificado.
        /// </summary>
        /// <param name="id">ID da transação a verificar.</param>
        /// <returns>Retorna true se a transação existir; retorna false caso contrário.</returns>

        private bool TransacaoExists(int id)
        {
            return _context.Transacoes.Any(e => e.TransacaoId == id);
        }


        /// <summary>
        /// Obtém o histórico de transações de um utilizador específico, filtrando por transações de ajuda, empréstimo e venda.
        /// </summary>
        /// <param name="utilizadorId">ID do utilizador para o qual o histórico de transações será recuperado.</param>
        /// <returns>Retorna uma lista do histórico de transações, com o tipo, data e número de carências transferidas. Em caso de falha, retorna 500 Internal Server Error.</returns>

        [HttpGet("Historico/{utilizadorId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetHistoricoTransacoes(int utilizadorId)
        {
            var transacoesAjuda = await _context.Transacoes
                .Include(t => t.TransacaoAjuda)
                .ThenInclude(ta => ta.PedidoAjuda)
                .Where(t => t.TransacaoAjuda != null && t.TransacaoAjuda.RecetorTran == utilizadorId)
                .ToListAsync();

            var transacoesEmprestimo = await _context.Transacoes
                .Include(t => t.TransacaoEmprestimo)
                .ThenInclude(te => te.Emprestimos)
                .ThenInclude(e => e.Items)
                .Where(t => t.TransacaoEmprestimo != null &&
                       (t.TransacaoEmprestimo.RecetorTran == utilizadorId || t.TransacaoEmprestimo.PagaTran == utilizadorId))
                .ToListAsync();

            var transacoesVenda = await _context.Transacoes
                .Include(t => t.Venda)
                .ThenInclude(v => v.Artigos)
                .Where(t => t.Venda != null && t.Venda.UtilizadorId == utilizadorId)
                .ToListAsync();

            var historico = transacoesAjuda
                .Select(t => new
                {
                    t.TransacaoId,
                    Tipo = "Ajuda",
                    Data = ((DateTime)t.DataTransacao).ToString("dd/MM HH:mm"),
                    NumeroCarenciasTransferido = t.Quantidade,
                    Titulo = t.TransacaoAjuda.PedidoAjuda.FirstOrDefault()?.Titulo ?? "Sem título"
                })
                .Concat(transacoesEmprestimo.Select(t => new
                {
                    t.TransacaoId,
                    Tipo = "Emprestimo",
                    Data = ((DateTime)t.DataTransacao).ToString("dd/MM HH:mm"),
                    NumeroCarenciasTransferido = t.Quantidade,
                    Titulo = t.TransacaoEmprestimo.Emprestimos.FirstOrDefault()?.Items.FirstOrDefault()?.NomeItem ?? "Sem título"
                }))
                .Concat(transacoesVenda.Select(t => new
                {
                    t.TransacaoId,
                    Tipo = "Venda",
                    Data = ((DateTime)t.DataTransacao).ToString("dd/MM HH:mm"),
                    NumeroCarenciasTransferido = t.Quantidade,
                    Titulo = t.Venda.Artigos?.FirstOrDefault().NomeArtigo ?? "Sem título"
                }))
                .OrderByDescending(t => t.Data)
                .ToList();

            return historico;
        }
    }
}
