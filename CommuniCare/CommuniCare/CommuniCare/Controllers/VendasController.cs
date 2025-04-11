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
    public class VendasController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public VendasController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/Vendas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venda>>> GetVenda()
        {
            return await _context.Venda.ToListAsync();
        }

        // GET: api/Vendas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Venda>> GetVenda(int id)
        {
            var venda = await _context.Venda.FindAsync(id);

            if (venda == null)
            {
                return NotFound();
            }

            return venda;
        }

        // PUT: api/Vendas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenda(int id, Venda venda)
        {
            if (id != venda.TransacaoId)
            {
                return BadRequest();
            }

            _context.Entry(venda).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendaExists(id))
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

        // POST: api/Vendas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Venda>> PostVenda(Venda venda)
        {
            _context.Venda.Add(venda);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (VendaExists(venda.TransacaoId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetVenda", new { id = venda.TransacaoId }, venda);
        }

        // DELETE: api/Vendas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenda(int id)
        {
            var venda = await _context.Venda.FindAsync(id);
            if (venda == null)
            {
                return NotFound();
            }

            _context.Venda.Remove(venda);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VendaExists(int id)
        {
            return _context.Venda.Any(e => e.TransacaoId == id);
        }





        [HttpPost("comprar")]
        public async Task<IActionResult> Comprar([FromBody] List<int> artigosIds)
        {
            // Verificar se o utilizador está autenticado
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            // Obter ID do utilizador autenticado
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var user = await _context.Utilizadores.FindAsync(userId);
            if (user == null)
                return NotFound("Utilizador não encontrado.");

            // Buscar os artigos
            var artigos = await _context.Artigos
                .Where(a => artigosIds.Contains(a.ArtigoId))
                .ToListAsync();

            // Verificar se todos os artigos foram encontrados
            if (artigos.Count != artigosIds.Count)
                return BadRequest("Alguns artigos não foram encontrados.");

            // Calcular o custo total da compra (quantidade de pontos necessários)
            int totalCares = artigos.Sum(a => a.CustoCares ?? 0);

            // Verificar se o utilizador tem pontos suficientes
            if (user.NumCares < totalCares)
                return BadRequest("Pontos insuficientes.");

            // Iniciar uma transação para garantir consistência de dados
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Criar nova transação para a compra
                    var transacao = new Transacao
                    {
                        DataTransacao = DateTime.Now,
                        Quantidade = artigos.Count
                    };
                    _context.Transacaos.Add(transacao);
                    await _context.SaveChangesAsync();

                    // Criar a venda
                    var venda = new Venda
                    {
                        UtilizadorId = user.UtilizadorId,
                        TransacaoId = transacao.TransacaoId,
                        NArtigos = artigos.Count
                    };
                    _context.Venda.Add(venda);
                    await _context.SaveChangesAsync();

                    // Associar os artigos à transação
                    foreach (var artigo in artigos)
                    {
                        artigo.TransacaoId = transacao.TransacaoId;
                    }

                    // Subtrair os pontos do utilizador
                    user.NumCares -= totalCares;

                    // Salvar as alterações na transação, venda e artigos
                    await _context.SaveChangesAsync();

                    // Commitar a transação
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        Sucesso = true,
                        Mensagem = "Compra efetuada com sucesso.",
                        TransacaoId = transacao.TransacaoId,
                        PontosRestantes = user.NumCares
                    });
                }
                catch (Exception)
                {
                    // Rollback em caso de erro
                    await transaction.RollbackAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar a compra.");
                }
            }
        }



    }




}
