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
    [Route("api/[controller]")]
    [ApiController]
    public class ArtigosController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public ArtigosController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/Artigos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Artigo>>> GetArtigos()
        {
            return await _context.Artigos.ToListAsync();
        }

        // GET: api/Artigos/5
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

        // PUT: api/Artigos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArtigo(int id, Artigo artigo)
        {
            if (id != artigo.ArtigoId)
            {
                return BadRequest();
            }

            _context.Entry(artigo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArtigoExists(id))
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

        // POST: api/Artigos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Artigo>> PostArtigo(ArtigoDto dto)
        {
            var artigo = new Artigo
            {
                NomeArtigo = dto.NomeArtigo,
                DescArtigo = dto.DescArtigo,
                CustoCares = dto.CustoCares,
                LojaId = dto.LojaId
            };

            _context.Artigos.Add(artigo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArtigo", new { id = artigo.ArtigoId }, artigo);
        }


        // DELETE: api/Artigos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArtigo(int id)
        {
            var artigo = await _context.Artigos.FindAsync(id);
            if (artigo == null)
            {
                return NotFound();
            }

            _context.Artigos.Remove(artigo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ArtigoExists(int id)
        {
            return _context.Artigos.Any(e => e.ArtigoId == id);
        }

        [HttpGet("disponiveis")]
        public async Task<ActionResult<IEnumerable<Artigo>>> GetArtigosDisponiveis()
        {
            var artigosDisponiveis = await _context.Artigos
                .Where(a => a.Estado == EstadoArtigo.Disponivel)
                .ToListAsync();

            return Ok(artigosDisponiveis);
        }

        [HttpPost("publicar")]
        public async Task<ActionResult<Artigo>> PublicarArtigo(ArtigoDto dto)
        {
            // Obter o ID do utilizador a partir do token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            // Converter o ID do utilizador para um número inteiro
            int utilizadorId = int.Parse(userIdClaim.Value);

            // Verificar se o utilizador é um administrador (TipoUtilizadorId == 2)
            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null || utilizador.TipoUtilizadorId != 2)
            {
                return Forbid("Apenas administradores podem publicar artigos.");
            }

            // Criar o artigo e definir o estado como 'Disponível'
            var artigo = new Artigo
            {
                NomeArtigo = dto.NomeArtigo,
                DescArtigo = dto.DescArtigo,
                CustoCares = dto.CustoCares,
                LojaId = dto.LojaId,
                Estado = EstadoArtigo.Disponivel // Define o estado como "Disponível"
            };

            // Adicionar o artigo à base de dados
            _context.Artigos.Add(artigo);
            await _context.SaveChangesAsync();

            // Retornar a resposta de sucesso
            return CreatedAtAction(nameof(GetArtigosDisponiveis), new { id = artigo.ArtigoId }, artigo);
        }

        [Authorize]
        [HttpPut("indisponibilizar/{id}")]
        public async Task<IActionResult> IndisponibilizarArtigo(int id)
        {
            // Obter ID do utilizador autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            // Verificar se o utilizador é administrador
            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null || utilizador.TipoUtilizadorId != 2)
            {
                return Forbid("Apenas administradores podem indisponibilizar artigos.");
            }

            // Procurar o artigo
            var artigo = await _context.Artigos.FindAsync(id);
            if (artigo == null)
            {
                return NotFound("Artigo não encontrado.");
            }

            // Alterar o estado do artigo
            artigo.Estado = EstadoArtigo.Indisponivel;

            // Guardar as alterações
            _context.Artigos.Update(artigo);
            await _context.SaveChangesAsync();

            return Ok("Artigo indisponibilizado com sucesso.");
        }

    }
}
