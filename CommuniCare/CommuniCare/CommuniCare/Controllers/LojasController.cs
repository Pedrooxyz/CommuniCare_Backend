using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommuniCare.Models;
using CommuniCare.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CommuniCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LojasController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public LojasController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a lista de todas as lojas no sistema.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="Loja"/> representando todas as lojas.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loja>>> GetLojas()
        {
            return await _context.Lojas.ToListAsync();
        }

        /// <summary>
        /// Obtém os detalhes de uma loja específica com base no identificador.
        /// </summary>
        /// <param name="id">Identificador único da loja.</param>
        /// <returns>Um objeto <see cref="Loja"/> com os dados da loja especificada, ou NotFound se a loja não existir.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Loja>> GetLoja(int id)
        {
            var loja = await _context.Lojas.FindAsync(id);

            if (loja == null)
            {
                return NotFound();
            }

            return loja;
        }

        /// <summary>
        /// Atualiza os dados de uma loja existente.
        /// </summary>
        /// <param name="id">Identificador da loja a ser atualizada.</param>
        /// <param name="loja">Objeto <see cref="Loja"/> contendo os dados atualizados da loja.</param>
        /// <returns>Um status de resposta que indica o sucesso ou falha da operação.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoja(int id, Loja loja)
        {
            if (id != loja.LojaId)
            {
                return BadRequest();
            }

            _context.Entry(loja).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LojaExists(id))
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
        /// Cria uma nova loja no sistema.
        /// </summary>
        /// <param name="loja">Objeto <see cref="Loja"/> com os dados da loja a ser criada.</param>
        /// <returns>O objeto <see cref="Loja"/> criado, incluindo o identificador da loja.</returns>
        [HttpPost]
        public async Task<ActionResult<Loja>> PostLoja(Loja loja)
        {
            _context.Lojas.Add(loja);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLoja", new { id = loja.LojaId }, loja);
        }

        /// <summary>
        /// Exclui uma loja do sistema com base no identificador.
        /// </summary>
        /// <param name="id">Identificador único da loja a ser excluída.</param>
        /// <returns>Um status de resposta que indica o sucesso ou falha da operação.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoja(int id)
        {
            var loja = await _context.Lojas.FindAsync(id);
            if (loja == null)
            {
                return NotFound();
            }

            _context.Lojas.Remove(loja);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LojaExists(int id)
        {
            return _context.Lojas.Any(e => e.LojaId == id);
        }


        /// <summary>
        /// Cria uma nova loja com validação de utilizador autenticado e permissões específicas.
        /// </summary>
        /// <param name="lojaDto">Objeto <see cref="LojaDto"/> com os dados para criar uma nova loja.</param>
        /// <returns>O objeto <see cref="Loja"/> criado com o estado "Ativo".</returns>
        /// <remarks>Requer que o utilizador esteja autenticado e seja do tipo 2 (admin).</remarks>
        [HttpPost("criar-loja")]
        [Authorize]
        public async Task<ActionResult> CriarLoja([FromBody] LojaDto lojaDto)
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
                return Forbid("Apenas utilizadores do tipo 2 podem validar devoluções.");
            }

            var lojasAtivas = _context.Lojas.Where(l => l.Estado == EstadoLoja.Ativo);
            foreach (var loja in lojasAtivas)
            {
                loja.Estado = EstadoLoja.Inativo;
            }

            var novaLoja = new Loja
            {
                NomeLoja = lojaDto.NomeLoja,
                DescLoja = lojaDto.DescLoja,
                Estado = EstadoLoja.Ativo
            };

            _context.Lojas.Add(novaLoja);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLoja", new { id = novaLoja.LojaId }, new
            {
                lojaId = novaLoja.LojaId,
                nomeLoja = novaLoja.NomeLoja,
                descLoja = novaLoja.DescLoja,
                estado = novaLoja.Estado.ToString()
            });
        }

        /// <summary>
        /// Ativa uma loja e desativa todas as outras lojas no sistema.
        /// </summary>
        /// <param name="id">Identificador único da loja a ser ativada.</param>
        /// <returns>Um status indicando que a loja foi ativada com sucesso, ou erro se a loja não for encontrada.</returns>
        /// <remarks>Requer autenticação e permissões de administrador para realizar a ativação.</remarks>
        [HttpPut("ativar-loja/{id}")]
        [Authorize]
        public async Task<ActionResult> AtivarLoja(int id)
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
                return Forbid("Apenas administradores podem ativar lojas.");
            }

            var lojaParaAtivar = await _context.Lojas.FindAsync(id);
            if (lojaParaAtivar == null)
            {
                return NotFound("Loja não encontrada.");
            }

            var lojas = await _context.Lojas.ToListAsync();
            foreach (var loja in lojas)
            {
                loja.Estado = (loja.LojaId == id) ? EstadoLoja.Ativo : EstadoLoja.Inativo;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Loja ativada com sucesso.",
                lojaId = lojaParaAtivar.LojaId,
                estado = lojaParaAtivar.Estado.ToString()
            });
        }




    }
}
