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


        /// <summary>
        /// Obtém a lista de todos os empréstimos registados.
        /// </summary>
        /// <returns>Lista de empréstimos.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Emprestimo>>> GetEmprestimos()
        {
            return await _context.Emprestimos.ToListAsync();
        }


        #region CONTROLLERS AUTOMÁTICOS
        ///// <summary>
        ///// Obtém os detalhes de um empréstimo específico.
        ///// </summary>
        ///// <param name="id">ID do empréstimo.</param>
        ///// <returns>Detalhes do empréstimo.</returns>
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Emprestimo>> GetEmprestimo(int id)
        //{
        //    var emprestimo = await _context.Emprestimos.FindAsync(id);

        //    if (emprestimo == null)
        //    {
        //        return NotFound();
        //    }

        //    return emprestimo;
        //}

        ///// <summary>
        ///// Atualiza os dados de um empréstimo existente.
        ///// </summary>
        ///// <param name="id">ID do empréstimo a atualizar.</param>
        ///// <param name="emprestimo">Objeto empréstimo com os novos dados.</param>
        ///// <returns>Resposta de sucesso ou erro.</returns>
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutEmprestimo(int id, Emprestimo emprestimo)
        //{
        //    if (id != emprestimo.EmprestimoId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(emprestimo).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!EmprestimoExists(id))
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

        ///// <summary>
        ///// Regista um novo empréstimo.
        ///// </summary>
        ///// <param name="emprestimo">Objeto empréstimo a ser criado.</param>
        ///// <returns>Empréstimo criado com detalhes.</returns>
        //[HttpPost]
        //public async Task<ActionResult<Emprestimo>> PostEmprestimo(Emprestimo emprestimo)
        //{
        //    _context.Emprestimos.Add(emprestimo);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetEmprestimo", new { id = emprestimo.EmprestimoId }, emprestimo);
        //}

        ///// <summary>
        ///// Elimina um empréstimo existente.
        ///// </summary>
        ///// <param name="id">ID do empréstimo a eliminar.</param>
        ///// <returns>Resposta de sucesso ou erro.</returns>
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteEmprestimo(int id)
        //{
        //    var emprestimo = await _context.Emprestimos.FindAsync(id);
        //    if (emprestimo == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Emprestimos.Remove(emprestimo);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}
        #endregion


        private bool EmprestimoExists(int id)
        {
            return _context.Emprestimos.Any(e => e.EmprestimoId == id);
        }

        /// <summary>
        /// Regista a devolução de um item de empréstimo.
        /// </summary>
        /// <param name="emprestimoId">ID do empréstimo a concluir.</param>
        /// <returns>Confirmação de devolução registada e notificação enviada aos administradores.</returns>
        [HttpPost("DevolucaoItem/{emprestimoId}")]
        [Authorize]
        public async Task<IActionResult> ConcluirEmprestimo(int emprestimoId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            var emprestimo = await _context.Emprestimos
                .Include(e => e.Items)
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

            var admins = await _context.Utilizadores
                .Where(u => u.TipoUtilizadorId == 2)
                .ToListAsync();

            foreach (var admin in admins)
            {
                var notificacao = new Notificacao
                {
                    UtilizadorId = admin.UtilizadorId,
                    Mensagem = $"O empréstimo de item '{emprestimo.Items.FirstOrDefault()?.NomeItem}' foi concluído. Por favor, valide a devolução.",
                    Lida = 0,
                    DataMensagem = DateTime.Now,
                    ItemId = emprestimo.Items.FirstOrDefault()?.ItemId
                };

                _context.Notificacaos.Add(notificacao);
            }

            await _context.SaveChangesAsync();

            return Ok("Data de devolução registada com sucesso. Notificação enviada aos administradores para validar a conclusão.");
        }


        #region Administrador

        /// <summary>
        /// Valida o início de um empréstimo (admin).
        /// </summary>
        /// <param name="emprestimoId">ID do empréstimo a validar.</param>
        /// <returns>Confirmação de validação e notificações enviadas ao comprador e dono do item.</returns>
        [HttpPost("ValidarEmprestimo-(admin)/{emprestimoId}")]
        [Authorize]
        public async Task<IActionResult> ValidarEmprestimo(int emprestimoId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int adminId = int.Parse(userIdClaim.Value);

            var admin = await _context.Utilizadores.FindAsync(adminId);
            if (admin == null || admin.TipoUtilizadorId != 2)
                return Forbid("Apenas administradores podem validar empréstimos.");

            var emprestimo = await _context.Emprestimos
                .Include(e => e.Items)
                .FirstOrDefaultAsync(e => e.EmprestimoId == emprestimoId);

            if (emprestimo == null)
                return NotFound("Empréstimo não encontrado.");

            if (emprestimo.DataIni != null)
                return BadRequest("Este empréstimo já foi validado.");

            emprestimo.DataIni = DateTime.UtcNow;

            var item = emprestimo.Items.FirstOrDefault();
            if (item == null)
                return BadRequest("Item associado não encontrado.");

            var relacoes = await _context.ItemEmprestimoUtilizadores
                .Where(r => r.ItemId == item.ItemId)
                .ToListAsync();

            var compradorId = relacoes.FirstOrDefault(r => r.TipoRelacao == "Comprador")?.UtilizadorId;
            var donoId = relacoes.FirstOrDefault(r => r.TipoRelacao == "Dono")?.UtilizadorId;

            if (compradorId == null || donoId == null)
                return BadRequest("Relações de comprador ou dono não encontradas.");

            var notificacaoComprador = new Notificacao
            {
                UtilizadorId = compradorId.Value,
                Mensagem = $"O teu pedido de empréstimo para o item '{item.NomeItem}' foi validado.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                ItemId = item.ItemId
            };
            _context.Notificacaos.Add(notificacaoComprador);

            var notificacaoDono = new Notificacao
            {
                UtilizadorId = donoId.Value,
                Mensagem = $"O teu item '{item.NomeItem}' foi requisitado e a requisição foi validada.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                ItemId = item.ItemId
            };
            _context.Notificacaos.Add(notificacaoDono);

            await _context.SaveChangesAsync();

            return Ok("Empréstimo validado e notificações enviadas.");
        }

        /// <summary>
        /// Rejeita um pedido de empréstimo (admin).
        /// </summary>
        /// <param name="emprestimoId">ID do empréstimo a rejeitar.</param>
        /// <returns>Confirmação de rejeição, remoção das relações e notificação ao comprador.</returns>
        [HttpPost("RejeitarEmprestimo-(admin)/{emprestimoId}")]
        [Authorize]
        public async Task<IActionResult> RejeitarEmprestimo(int emprestimoId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int adminId = int.Parse(userIdClaim.Value);

            var admin = await _context.Utilizadores.FindAsync(adminId);
            if (admin == null || admin.TipoUtilizadorId != 2)
                return Forbid("Apenas administradores podem rejeitar empréstimos.");

            var emprestimo = await _context.Emprestimos
                .Include(e => e.Items)
                .FirstOrDefaultAsync(e => e.EmprestimoId == emprestimoId);

            if (emprestimo == null)
                return NotFound("Empréstimo não encontrado.");

            if (emprestimo.DataIni != null)
                return BadRequest("Este empréstimo já foi validado e não pode ser rejeitado.");

            var item = emprestimo.Items.FirstOrDefault();
            if (item == null)
                return BadRequest("Item associado não encontrado.");


            var relacaoComprador = await _context.ItemEmprestimoUtilizadores
                .FirstOrDefaultAsync(r => r.EmprestimoId == emprestimoId && r.TipoRelacao == "Comprador");

            if (relacaoComprador == null)
                return BadRequest("Relação do comprador não encontrada.");


            var notificacao = new Notificacao
            {
                UtilizadorId = relacaoComprador.UtilizadorId,
                Mensagem = $"O teu pedido de empréstimo para o item '{item.NomeItem}' foi rejeitado.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                ItemId = item.ItemId
            };
            _context.Notificacaos.Add(notificacao);


            item.Disponivel = EstadoItemEmprestimo.Disponivel;


            var relacoesCompradorRemover = await _context.ItemEmprestimoUtilizadores
                .Where(r => r.EmprestimoId == emprestimoId && r.TipoRelacao == "Comprador")
                .ToListAsync();
            _context.ItemEmprestimoUtilizadores.RemoveRange(relacoesCompradorRemover);


            emprestimo.Items.Clear();
            _context.Emprestimos.Remove(emprestimo);


            await _context.SaveChangesAsync();

            return Ok("Empréstimo rejeitado, relações removidas e notificação enviada.");
        }


        /// <summary>
        /// Valida a devolução de um empréstimo (admin).
        /// </summary>
        /// <param name="emprestimoId">ID do empréstimo cuja devolução será validada.</param>
        /// <returns>Confirmação de devolução validada, transações registadas e notificações enviadas.</returns>
        [HttpPost("ValidarDevolucao-(admin)/{emprestimoId}")]
        [Authorize]
        public async Task<IActionResult> ValidarDevolucaoEmprestimo(int emprestimoId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null || utilizador.TipoUtilizadorId != 2)
            {
                return Forbid("Apenas administradores podem validar devoluções.");
            }

            var emprestimo = await _context.Emprestimos
                .Include(e => e.Items)
                    .ThenInclude(i => i.ItemEmprestimoUtilizadores)
                        .ThenInclude(rel => rel.Utilizador)
                .FirstOrDefaultAsync(e => e.EmprestimoId == emprestimoId);

            if (emprestimo == null)
            {
                return NotFound("Empréstimo não encontrado.");
            }

            if (!emprestimo.DataDev.HasValue || !emprestimo.DataIni.HasValue)
            {
                return BadRequest("Data de devolução ou data de início inválida.");
            }

            var notificacoes = new List<Notificacao>();
            var transacoes = new List<Transacao>();
            var transacoesEmprestimo = new List<TransacaoEmprestimo>();

            TimeSpan diferenca = emprestimo.DataDev.Value - emprestimo.DataIni.Value;
            int nHoras = Math.Max(1, (int)Math.Ceiling(diferenca.TotalHours));

            foreach (var item in emprestimo.Items)
            {
                item.Disponivel = EstadoItemEmprestimo.Disponivel;

                var comprador = item.ItemEmprestimoUtilizadores
                    .FirstOrDefault(rel => rel.TipoRelacao == "Comprador")?.Utilizador;
                var dono = item.ItemEmprestimoUtilizadores
                    .FirstOrDefault(rel => rel.TipoRelacao == "Dono")?.Utilizador;

                if (comprador == null || dono == null)
                {
                    return BadRequest("Não foi possível determinar o dono ou o comprador de um dos itens.");
                }

                int comissaoItem = item.ComissaoCares ?? 0;
                int quantidadeCares = nHoras * comissaoItem;

                if (comprador.NumCares < quantidadeCares)
                {
                    return BadRequest($"O comprador (ID: {comprador.UtilizadorId}) não tem cares suficientes para pagar pelo item {item.ItemId}.");
                }

                comprador.NumCares -= quantidadeCares;
                dono.NumCares += quantidadeCares;

                var transacao = new Transacao
                {
                    DataTransacao = DateTime.UtcNow,
                    Quantidade = quantidadeCares
                };
                transacoes.Add(transacao);

                var transacaoEmprestimo = new TransacaoEmprestimo
                {
                    NHoras = nHoras,
                    RecetorTran = dono.UtilizadorId,
                    PagaTran = comprador.UtilizadorId,
                    Transacao = transacao,
                    Emprestimos = new List<Emprestimo> { emprestimo }
                };
                transacoesEmprestimo.Add(transacaoEmprestimo);

                notificacoes.Add(new Notificacao
                {
                    UtilizadorId = dono.UtilizadorId,
                    Mensagem = $"Os cares no valor de {quantidadeCares} foram enviados para sua conta pelo item {item.ItemId}."
                });

                notificacoes.Add(new Notificacao
                {
                    UtilizadorId = comprador.UtilizadorId,
                    Mensagem = $"A devolução do item {item.ItemId} foi validada e {quantidadeCares} cares foram deduzidos da sua conta."
                });
            }

            _context.Transacoes.AddRange(transacoes);
            _context.TransacoesEmprestimo.AddRange(transacoesEmprestimo);
            _context.Notificacaos.AddRange(notificacoes);

            await _context.SaveChangesAsync();

            return Ok("Empréstimo validado, saldo atualizado e transações registadas.");
        }



        #endregion

    }
}
