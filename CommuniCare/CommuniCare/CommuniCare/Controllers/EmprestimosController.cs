using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommuniCare.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CommuniCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmprestimosController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public EmprestimosController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/Emprestimos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Emprestimo>>> GetEmprestimos()
        {
            return await _context.Emprestimos.ToListAsync();
        }

        // GET: api/Emprestimos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Emprestimo>> GetEmprestimo(int id)
        {
            var emprestimo = await _context.Emprestimos.FindAsync(id);

            if (emprestimo == null)
            {
                return NotFound();
            }

            return emprestimo;
        }

        // PUT: api/Emprestimos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmprestimo(int id, Emprestimo emprestimo)
        {
            if (id != emprestimo.EmprestimoId)
            {
                return BadRequest();
            }

            _context.Entry(emprestimo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmprestimoExists(id))
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

        // POST: api/Emprestimos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Emprestimo>> PostEmprestimo(Emprestimo emprestimo)
        {
            _context.Emprestimos.Add(emprestimo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmprestimo", new { id = emprestimo.EmprestimoId }, emprestimo);
        }

        // DELETE: api/Emprestimos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmprestimo(int id)
        {
            var emprestimo = await _context.Emprestimos.FindAsync(id);
            if (emprestimo == null)
            {
                return NotFound();
            }

            _context.Emprestimos.Remove(emprestimo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmprestimoExists(int id)
        {
            return _context.Emprestimos.Any(e => e.EmprestimoId == id);
        }

        [HttpPost("devolucao-item/{emprestimoId}")]
        [Authorize]
        public async Task<IActionResult> ConcluirEmprestimo(int emprestimoId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            var emprestimo = await _context.Emprestimos
                .FirstOrDefaultAsync(e => e.EmprestimoId == emprestimoId);

            if (emprestimo == null)
            {
                return NotFound("Empréstimo não encontrado.");
            }

            if (emprestimo.DataDev != null)
            {
                return BadRequest("Este empréstimo já foi concluído.");
            }

            emprestimo.DataDev = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Data de devolução registada com sucesso.");
        }

        //[HttpPost("validar-devolucao/{emprestimoId}")]
        //[Authorize]
        //public async Task<IActionResult> ValidarDevolucaoEmprestimo(int emprestimoId)
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        //    if (userIdClaim == null)
        //    {
        //        return Unauthorized("Utilizador não autenticado.");
        //    }

        //    int utilizadorId = int.Parse(userIdClaim.Value);

        //    var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
        //    if (utilizador == null || utilizador.TipoUtilizadorId != 2)
        //    {
        //        return Forbid("Apenas utilizadores do tipo 2 podem validar devoluções.");
        //    }

        //    var emprestimo = await _context.Emprestimos
        //        .Include(e => e.Items)
        //            .ThenInclude(i => i.Utilizadores) // Aceder ao(s) dono(s) dos itens
        //        .FirstOrDefaultAsync(e => e.EmprestimoId == emprestimoId);

        //    if (emprestimo == null)
        //    {
        //        return NotFound("Empréstimo não encontrado.");
        //    }

        //    if (emprestimo.DataDev != null)
        //    {
        //        return BadRequest("Este empréstimo já foi concluído.");
        //    }

        //    // Registar a data de devolução
        //    emprestimo.DataDev = DateTime.UtcNow;

        //    // Tornar todos os itens disponíveis novamente
        //    foreach (var itemEmprestimo in emprestimo.Items)
        //    {
        //        itemEmprestimo.Disponivel = 1;
        //    }

        //    // Calcular horas de empréstimo (mínimo de 1 hora)
        //    int nHoras = (int)Math.Max(1, (emprestimo.DataDev.Value - emprestimo.DataIni).TotalHours);

        //    // Calcular total da comissão
        //    int totalComissao = emprestimo.Items
        //        .Sum(i => i.ComissaoCares ?? 0);

        //    // Obter dono do primeiro item (quem emprestou)
        //    var primeiroItem = emprestimo.Items.FirstOrDefault();
        //    int? recetorId = primeiroItem?.Utilizadores.FirstOrDefault(u => u.UtilizadorId != emprestimo.UtilizadorId)?.UtilizadorId;

        //    // Quem requisitou (quem pagará)
        //    int? pagaId = emprestimo.UtilizadorId;

        //    var transacao = new Transacao
        //    {
        //        DataTransacao = DateTime.UtcNow,
        //        Quantidade = nHoras * totalComissao
        //    };

        //    var transacaoEmprestimo = new TransacaoEmprestimo
        //    {
        //        NHoras = nHoras,
        //        RecetorTran = recetorId,
        //        PagaTran = pagaId,
        //        Transacao = transacao,
        //        Emprestimos = new List<Emprestimo> { emprestimo }
        //    };

        //    _context.Transacoes.Add(transacao);
        //    _context.TransacoesEmprestimo.Add(transacaoEmprestimo);

        //    await _context.SaveChangesAsync();

        //    return Ok("Empréstimo validado, itens atualizados e transação registada.");
        //}



    }
}
