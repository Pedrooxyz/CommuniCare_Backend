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
                .Include(e => e.Items)  // Caso precise acessar detalhes do item do empréstimo
                .FirstOrDefaultAsync(e => e.EmprestimoId == emprestimoId);

            if (emprestimo == null)
            {
                return NotFound("Empréstimo não encontrado.");
            }

            if (emprestimo.DataDev != null)
            {
                return BadRequest("Este empréstimo já foi concluído.");
            }

            // Registra a data de devolução
            emprestimo.DataDev = DateTime.UtcNow;

            // Notificar os administradores para validar a devolução
            var admins = await _context.Utilizadores
                .Where(u => u.TipoUtilizadorId == 2) // Considerando que TipoUtilizadorId 2 é para administradores
                .ToListAsync();

            foreach (var admin in admins)
            {
                var notificacao = new Notificacao
                {
                    UtilizadorId = admin.UtilizadorId,
                    Mensagem = $"O empréstimo de item '{emprestimo.Items.FirstOrDefault()?.NomeItem}' foi concluído. Por favor, valide a devolução.",
                    Lida = 0,
                    DataMensagem = DateTime.Now,
                    ItemId = emprestimo.Items.FirstOrDefault()?.ItemId // Ou algum outro campo que você precise para o item
                };

                _context.Notificacaos.Add(notificacao);
            }

            await _context.SaveChangesAsync();

            return Ok("Data de devolução registada com sucesso. Notificação enviada aos administradores para validar a conclusão.");
        }


        #region Administrador


        [HttpPost("validar-emprestimo/{emprestimoId}")]
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
                .Include(e => e.Items)  // Certificando que a coleção de itens é carregada
                .FirstOrDefaultAsync(e => e.EmprestimoId == emprestimoId);

            if (emprestimo == null)
                return NotFound("Empréstimo não encontrado.");

            if (emprestimo.DataIni != null)
                return BadRequest("Este empréstimo já foi validado.");

            emprestimo.DataIni = DateTime.UtcNow;

            var item = emprestimo.Items.FirstOrDefault();  // Pegando o primeiro item associado ao empréstimo
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

        [HttpPost("rejeitar-emprestimo/{emprestimoId}")]
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
                .Include(e => e.Items)  // Incluindo os itens do empréstimo
                .FirstOrDefaultAsync(e => e.EmprestimoId == emprestimoId);

            if (emprestimo == null)
                return NotFound("Empréstimo não encontrado.");

            if (emprestimo.DataIni != null)
                return BadRequest("Este empréstimo já foi validado e não pode ser rejeitado.");

            var item = emprestimo.Items.FirstOrDefault();
            if (item == null)
                return BadRequest("Item associado não encontrado.");

            // Buscar relação de comprador pelo emprestimoId
            var relacaoComprador = await _context.ItemEmprestimoUtilizadores
                .FirstOrDefaultAsync(r => r.EmprestimoId == emprestimoId && r.TipoRelacao == "Comprador");

            if (relacaoComprador == null)
                return BadRequest("Relação do comprador não encontrada.");

            // Criação da notificação
            var notificacao = new Notificacao
            {
                UtilizadorId = relacaoComprador.UtilizadorId,
                Mensagem = $"O teu pedido de empréstimo para o item '{item.NomeItem}' foi rejeitado.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                ItemId = item.ItemId
            };
            _context.Notificacaos.Add(notificacao);

            // Marca o item como disponível novamente
            item.Disponivel = 1;

            // Remove as relações de comprador
            var relacoesCompradorRemover = await _context.ItemEmprestimoUtilizadores
                .Where(r => r.EmprestimoId == emprestimoId && r.TipoRelacao == "Comprador")
                .ToListAsync();
            _context.ItemEmprestimoUtilizadores.RemoveRange(relacoesCompradorRemover);

            // Remove o empréstimo e seus itens
            emprestimo.Items.Clear(); // Remover relações de itens
            _context.Emprestimos.Remove(emprestimo);

            // Salvar alterações no banco de dados
            await _context.SaveChangesAsync();

            return Ok("Empréstimo rejeitado, relações removidas e notificação enviada.");
        }

        [HttpPost("validar-devolucao/{emprestimoId}")]
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
                item.Disponivel = 1;

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
