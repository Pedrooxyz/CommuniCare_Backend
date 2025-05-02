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
    public class FavoritosController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public FavoritosController(CommuniCareContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Adiciona um artigo aos favoritos do utilizador.
        /// </summary>
        /// <param name="artigoId">ID do artigo a ser adicionado aos favoritos.</param>
        /// <returns>Retorna 204 (No Content) se o artigo for adicionado com sucesso ou 404 (Not Found) se o artigo não existir.</returns

        [HttpPost("{artigoId:int}")]
        public async Task<IActionResult> AddFavorite(int artigoId)
        {
            int userId = User.GetUserId();

            bool artigoExiste = await _context.Artigos
                .AnyAsync(a => a.ArtigoId == artigoId);

            if (!artigoExiste)
                return NotFound($"O artigo {artigoId} não existe.");

            bool jaFavorito = await _context.Favoritos
                .AnyAsync(f => f.UtilizadorId == userId && f.ArtigoId == artigoId);

            if (jaFavorito)
                return Conflict("Já está nos favoritos.");


            _context.Favoritos.Add(new Favoritos
            {
                UtilizadorId = userId,
                ArtigoId = artigoId
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Remove um artigo dos favoritos do utilizador.
        /// </summary>
        /// <param name="artigoId">ID do artigo a ser removido dos favoritos.</param>
        /// <returns>Retorna 204 (No Content) se o artigo for removido com sucesso, 404 (Not Found) se o artigo não existir ou se o artigo não estiver nos favoritos.</returns>
        [HttpDelete("{artigoId:int}")]
        public async Task<IActionResult> RemoveFavorite(int artigoId)
        {
            int userId = User.GetUserId();


            bool artigoExiste = await _context.Artigos
                .AnyAsync(a => a.ArtigoId == artigoId);

            if (!artigoExiste)
                return NotFound($"O artigo {artigoId} não existe.");


            var favorito = await _context.Favoritos.FindAsync(userId, artigoId);

            if (favorito is null)
                return NotFound("Este artigo não está nos teus favoritos.");


            _context.Favoritos.Remove(favorito);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Lista todos os artigos favoritos de um utilizador.
        /// </summary>
        /// <returns>Retorna uma lista de artigos favoritos do utilizador com nome, descrição, custo em cares e quantidade disponível.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArtigoDto>>> ListFavorites()
        {
            int userId = User.GetUserId();

            var artigos = await _context.Favoritos
                .Where(f => f.UtilizadorId == userId)
                .Select(f => new ArtigoDto
                {
                    NomeArtigo = f.Artigo.NomeArtigo,
                    DescArtigo = f.Artigo.DescArtigo,
                    CustoCares = f.Artigo.CustoCares,
                    QuantidadeDisponivel = f.Artigo.QuantidadeDisponivel
                })
                .ToListAsync();

            return Ok(artigos);
        }
    }


}

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedAccessException("User id claim missing.");
        return int.Parse(value);
    }
}

