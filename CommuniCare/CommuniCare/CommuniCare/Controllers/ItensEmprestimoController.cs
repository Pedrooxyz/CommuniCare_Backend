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
                Disponivel = 1,  // Disponível como true
                ComissaoCares = itemData.ComissaoCares
            };

            // Adicionar o item ao contexto
            _context.ItensEmprestimo.Add(itemEmprestimo);

            // Salvar no banco de dados
            await _context.SaveChangesAsync();

            // Associar o item com o utilizador
            utilizador.ItensEmprestimo.Add(itemEmprestimo);
            await _context.SaveChangesAsync();

            return Ok("Item de empréstimo adicionado com sucesso.");
        }


        /**
         * É necessário na funcao abaixo criar uma instancia da classe emprestimo e alem disso a mais algumas coisas a 
         * preocupar: ao adquirir alem de quere que nos icollections adiciones o utilizador associado ao token que fez 
         * a aquisição quero também que cries uma instancia da classe Emprestimo em que o UtilizadorId seja igual ao utilazadorId 
         * da tabela ItemEmprestimo
         */
        [HttpPost("adquirir-item/{itemId}")]
        [Authorize] // Garante que só utilizadores autenticados podem aceder
        public async Task<IActionResult> AdquirirItem(int itemId)
        {
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

            // Encontrar o item de empréstimo pelo ID
            var itemEmprestimo = await _context.ItensEmprestimo.FindAsync(itemId);
            if (itemEmprestimo == null)
            {
                return NotFound("Item de empréstimo não encontrado.");
            }

            // Verificar se o utilizador que está pedindo o item não é o mesmo que colocou o item
            if (itemEmprestimo.Utilizadores.Any(u => u.UtilizadorId == utilizadorId))
            {
                return BadRequest("Não pode adquirir o item que você mesmo colocou.");
            }

            // Verificar se o item está disponível
            if (itemEmprestimo.Disponivel == 0)
            {
                return BadRequest("Este item não está disponível para empréstimo.");
            }

            // Marcar o item como indisponível
            itemEmprestimo.Disponivel = 0;

            // Associar o item ao utilizador que está pedindo
            itemEmprestimo.Utilizadores.Add(utilizador);

            // Salvar no banco de dados
            await _context.SaveChangesAsync();

            return Ok("Item adquirido com sucesso.");
        }

    }
}
