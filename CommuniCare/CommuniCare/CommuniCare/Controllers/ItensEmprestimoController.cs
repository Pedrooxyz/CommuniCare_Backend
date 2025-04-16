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
using CommuniCare.DTOs;

namespace CommuniCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItensEmprestimoController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public ItensEmprestimoController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/ItemEmprestimoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemEmprestimo>>> GetItemEmprestimos()
        {
            return await _context.ItensEmprestimo.ToListAsync();
        }

        // GET: api/ItemEmprestimoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemEmprestimo>> GetItemEmprestimo(int id)
        {
            var itemEmprestimo = await _context.ItensEmprestimo.FindAsync(id);

            if (itemEmprestimo == null)
            {
                return NotFound();
            }

            return itemEmprestimo;
        }

        // PUT: api/ItemEmprestimoes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItemEmprestimo(int id, ItemEmprestimo itemEmprestimo)
        {
            if (id != itemEmprestimo.ItemId)
            {
                return BadRequest();
            }

            _context.Entry(itemEmprestimo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemEmprestimoExists(id))
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

        // POST: api/ItemEmprestimoes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ItemEmprestimo>> PostItemEmprestimo(ItemEmprestimo itemEmprestimo)
        {
            _context.ItensEmprestimo.Add(itemEmprestimo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetItemEmprestimo", new { id = itemEmprestimo.ItemId }, itemEmprestimo);
        }

        // DELETE: api/ItemEmprestimoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemEmprestimo(int id)
        {
            var itemEmprestimo = await _context.ItensEmprestimo.FindAsync(id);
            if (itemEmprestimo == null)
            {
                return NotFound();
            }

            _context.ItensEmprestimo.Remove(itemEmprestimo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemEmprestimoExists(int id)
        {
            return _context.ItensEmprestimo.Any(e => e.ItemId == id);
        }

        [HttpPost("adicionar-item")]
        [Authorize] // Garante que só utilizadores autenticados podem aceder
        public async Task<IActionResult> AdicionarItemEmprestimo([FromBody] ItemEmprestimoDTO itemData)
        {
            if (itemData == null)
            {
                return BadRequest("Dados inválidos.");
            }

            // Obter o ID do utilizador autenticado a partir do JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            // Verificar se o utilizador existe
            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            // Criar o item de empréstimo
            var itemEmprestimo = new ItemEmprestimo
            {
                NomeItem = itemData.NomeItem,
                DescItem = itemData.DescItem,
                Disponivel = 1,
                ComissaoCares = itemData.ComissaoCares
            };

            _context.ItensEmprestimo.Add(itemEmprestimo);
            await _context.SaveChangesAsync();

            // Criar a relação explícita com TipoRelacao = "Dono"
            var relacao = new ItemEmprestimoUtilizador
            {
                ItemId = itemEmprestimo.ItemId,
                UtilizadorId = utilizadorId,
                TipoRelacao = "Dono"
            };

            _context.ItemEmprestimoUtilizadores.Add(relacao);
            await _context.SaveChangesAsync();

            return Ok("Item de empréstimo adicionado com sucesso.");
        }

        //Aqui temos o problema de um utilizador nao poder pedir emprestado duas vezes a mesma coisa porque isso
        //causaria a repeticao de atributos para nao acontecer isso acrecentamos outra PK que seja o id de cada instancia
        [HttpPost("adquirir-item/{itemId}")]
        [Authorize]
        public async Task<IActionResult> AdquirirItem(int itemId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            var itemEmprestimo = await _context.ItensEmprestimo
                .Include(i => i.Utilizadores) // Opcional, pode remover se não estiver a usar diretamente
                .FirstOrDefaultAsync(i => i.ItemId == itemId);

            if (itemEmprestimo == null)
            {
                return NotFound("Item de empréstimo não encontrado.");
            }

            // Verificar se o utilizador já está associado como Dono
            var jaEDono = await _context.ItemEmprestimoUtilizadores
                .AnyAsync(r => r.ItemId == itemId && r.UtilizadorId == utilizadorId && r.TipoRelacao == "Dono");

            if (jaEDono)
            {
                return BadRequest("Não pode adquirir o item que você mesmo colocou.");
            }

            if (itemEmprestimo.Disponivel == 0)
            {
                return BadRequest("Este item não está disponível para empréstimo.");
            }

            // Verificar se o utilizador tem Cares suficientes
            int comissao = itemEmprestimo.ComissaoCares ?? 0;
            if (utilizador.NumCares < comissao)
            {
                return BadRequest("Saldo de Cares insuficiente para adquirir este item.");
            }

            itemEmprestimo.Disponivel = 0;

            var emprestimo = new Emprestimo
            {
                DataIni = DateTime.UtcNow,
                Items = new List<ItemEmprestimo> { itemEmprestimo },
            };

            // Criar ligação com TipoRelacao = Requisitante
            var relacao = new ItemEmprestimoUtilizador
            {
                ItemId = itemId,
                UtilizadorId = utilizadorId,
                TipoRelacao = "Comprador"
                            
            };

            _context.Emprestimos.Add(emprestimo);
            _context.ItemEmprestimoUtilizadores.Add(relacao);
            _context.ItensEmprestimo.Update(itemEmprestimo);

            await _context.SaveChangesAsync();

            return Ok("Item adquirido com sucesso.");
        }

        [HttpGet("disponiveis")]
        public async Task<ActionResult<IEnumerable<ItemEmprestimo>>> GetItensDisponiveis()
        {
            var itensDisponiveis = await _context.ItensEmprestimo
                .Where(item => item.Disponivel == 1)
                .ToListAsync();

            return Ok(itensDisponiveis);
        }

        [HttpDelete("indisponibilizar-permanente-item/{itemId}")]
        [Authorize] 
        public async Task<IActionResult> EliminarItemEmprestimo(int itemId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId;
            if (!int.TryParse(userIdClaim.Value, out utilizadorId))
            {
                return Unauthorized("ID de utilizador inválido.");
            }

            var itemEmprestimoUtilizador = await _context.ItemEmprestimoUtilizadores
                .Include(ie => ie.ItemEmprestimo) 
                .Include(ie => ie.Utilizador) 
                .FirstOrDefaultAsync(ie => ie.ItemId == itemId && ie.UtilizadorId == utilizadorId);

            if (itemEmprestimoUtilizador == null)
            {
                return NotFound("Item de empréstimo ou relação de utilizador não encontrado.");
            }

            if (itemEmprestimoUtilizador.TipoRelacao != "Dono")
            {
                return Unauthorized("Você não tem permissão para alterar este item.");
            }

            var itemEmprestimo = await _context.ItensEmprestimo
                .FirstOrDefaultAsync(i => i.ItemId == itemId);


            if (itemEmprestimo == null)
            {
                return NotFound("Item de empréstimo não encontrado.");
            }

            itemEmprestimo.Disponivel = 0; 

            _context.ItensEmprestimo.Update(itemEmprestimo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
