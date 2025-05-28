using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommuniCare.Models;
using CommuniCare.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CommuniCare.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ArtigosController : ControllerBase
    {
        private readonly CommuniCareContext _context;


        /// <summary>
        /// Construtor do controlador de artigos.
        /// </summary>
        /// <param name="context">Contexto da base de dados CommuniCare utilizado para aceder aos artigos e outras entidades relacionadas.</param>
        public ArtigosController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a lista completa de todos os artigos existentes na base de dados, independentemente do seu estado.
        /// </summary>
        /// <returns>Uma lista de todos os artigos registados.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Artigo>>> GetArtigos()
        {
            return await _context.Artigos.ToListAsync();
        }


        #region CONTROLLERS AUTOMÁTICOS
        [HttpGet("{id}")]
        public async Task<ActionResult<Artigo>> GetArtigo(int id)
        {
            var artigo = await _context.Artigos.FindAsync(id);

            if (artigo == null)
            {
                return NotFound();
            }

            return artigo;
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutArtigo(int id, Artigo artigo)
        //{
        //    if (id != artigo.ArtigoId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(artigo).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ArtigoExists(id))
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

        //// POST: api/Artigos
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<Artigo>> PostArtigo(ArtigoDto dto)
        //{
        //    var artigo = new Artigo
        //    {
        //        NomeArtigo = dto.NomeArtigo,
        //        DescArtigo = dto.DescArtigo,
        //        CustoCares = dto.CustoCares,
        //        //LojaId = dto.LojaId,
        //        QuantidadeDisponivel = dto.QuantidadeDisponivel
        //    };

        //    _context.Artigos.Add(artigo);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetArtigo", new { id = artigo.ArtigoId }, artigo);
        //}


        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteArtigo(int id)
        //{
        //    var artigo = await _context.Artigos.FindAsync(id);
        //    if (artigo == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Artigos.Remove(artigo);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}
        #endregion


        private bool ArtigoExists(int id)
        {
            return _context.Artigos.Any(e => e.ArtigoId == id);
        }

        [HttpGet("Disponiveis")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ArtigoRespostaDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<ActionResult<IEnumerable<ArtigoRespostaDto>>> GetArtigosDisponiveis()
        {
            var lojaAtiva = await _context.Lojas.FirstOrDefaultAsync(l => l.Estado == EstadoLoja.Ativo);

            if (lojaAtiva == null)
            {
                return NotFound("Nenhuma loja ativa encontrada.");
            }

            var artigosDisponiveis = await _context.Artigos
                .Where(a => a.Estado == EstadoArtigo.Disponivel && a.LojaId == lojaAtiva.LojaId)
                .Select(a => new ArtigoRespostaDto
                {
                    ArtigoId = a.ArtigoId,
                    NomeArtigo = a.NomeArtigo,
                    DescArtigo = a.DescArtigo,
                    CustoCares = a.CustoCares,
                    QuantidadeDisponivel = a.QuantidadeDisponivel,
                    LojaId = a.LojaId,
                    FotografiaArt = a.FotografiaArt
                })
                .ToListAsync();

            return Ok(artigosDisponiveis);
        }




        /// <summary>
        /// Publica um novo artigo na loja ativa, associando-o ao utilizador autenticado.
        /// </summary>
        /// <param name="dto">Objeto que contém as informações do artigo a ser publicado.</param>
        /// <returns>O artigo publicado com os seus dados ou um erro caso as regras de negócio não sejam cumpridas.</returns>
        [HttpPost("Publicar-(admin)")]
        public async Task<ActionResult<ArtigoRespostaDto>> PublicarArtigo(ArtigoDto dto)
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

            var lojaAtiva = await _context.Lojas
                .FirstOrDefaultAsync(l => l.Estado == EstadoLoja.Ativo);

            if (lojaAtiva == null)
            {
                return BadRequest("Não existe nenhuma loja ativa no momento.");
            }

            var artigo = new Artigo
            {
                NomeArtigo = dto.NomeArtigo,
                DescArtigo = dto.DescArtigo,
                CustoCares = dto.CustoCares,
                LojaId = lojaAtiva.LojaId,
                QuantidadeDisponivel = dto.QuantidadeDisponivel,
                Estado = EstadoArtigo.Disponivel,
                FotografiaArt = dto.FotografiaArt
            };

            _context.Artigos.Add(artigo);
            await _context.SaveChangesAsync();

            var respostaDto = new ArtigoRespostaDto
            {
                ArtigoId = artigo.ArtigoId,
                NomeArtigo = artigo.NomeArtigo,
                DescArtigo = artigo.DescArtigo,
                CustoCares = artigo.CustoCares,
                QuantidadeDisponivel = artigo.QuantidadeDisponivel
            };

            return CreatedAtAction(nameof(GetArtigosDisponiveis), new { id = artigo.ArtigoId }, respostaDto);
        }


        /// <summary>
        /// Indisponibiliza um artigo, tornando-o não disponível para compra. Apenas administradores podem realizar esta ação.
        /// </summary>
        /// <param name="id">ID do artigo a ser indisponibilizado.</param>
        /// <returns>Confirmação de indisponibilização ou erro caso o artigo ou utilizador não sejam válidos.</returns>
        [Authorize]
        [HttpPut("Indisponibilizar-(admin)/{id}")]
        public async Task<IActionResult> IndisponibilizarArtigo(int id)
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
                return Forbid("Apenas administradores podem indisponibilizar artigos.");
            }

            var artigo = await _context.Artigos.FindAsync(id);
            if (artigo == null)
            {
                return NotFound("Artigo não encontrado.");
            }

            artigo.Estado = EstadoArtigo.Indisponivel;

            _context.Artigos.Update(artigo);
            await _context.SaveChangesAsync();

            return Ok("Artigo indisponibilizado com sucesso.");
        }


        /// <summary>
        /// Reposta o stock de um artigo existente, atualizando a quantidade disponível.
        /// </summary>
        /// <param name="id">ID do artigo cujo stock será reposto.</param>
        /// <param name="dto">Objeto que contém a quantidade a ser adicionada ao stock.</param>
        /// <returns>O artigo atualizado com a nova quantidade de stock ou erro caso as regras não sejam cumpridas.</returns>
        [HttpPut("{id}/Repor-stock-(admin)")]
        public async Task<ActionResult<ArtigoRespostaDto>> ReporStock(int id, [FromBody] ReporStockDto dto)
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

            var lojaAtiva = await _context.Lojas
                .FirstOrDefaultAsync(l => l.Estado == EstadoLoja.Ativo);
            if (lojaAtiva == null)
            {
                return BadRequest("Não existe nenhuma loja ativa no momento.");
            }

            if (dto.Quantidade <= 0)
            {
                return BadRequest("A quantidade deve ser maior que zero.");
            }

            var artigo = await _context.Artigos.FindAsync(id);
            if (artigo == null)
            {
                return NotFound("Artigo não encontrado.");
            }

            if (artigo.LojaId != lojaAtiva.LojaId)
            {
                return BadRequest("O artigo não pertence à loja ativa.");
            }

            try
            {
                artigo.QuantidadeDisponivel += dto.Quantidade;
                artigo.Estado = artigo.QuantidadeDisponivel > 0
                    ? EstadoArtigo.Disponivel
                    : EstadoArtigo.Indisponivel;

                _context.Entry(artigo).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var respostaDto = new ArtigoRespostaDto
                {
                    ArtigoId = artigo.ArtigoId,
                    NomeArtigo = artigo.NomeArtigo,
                    DescArtigo = artigo.DescArtigo,
                    CustoCares = artigo.CustoCares,
                    QuantidadeDisponivel = artigo.QuantidadeDisponivel
                };

                return Ok(respostaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao repor stock: {ex.Message}");
            }
        }

    }
}
