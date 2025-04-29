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



        [HttpPost("{artigoId:int}")]
        public async Task<IActionResult> AddFavorite(int artigoId)
        {
            int userId = User.GetUserId();

            /* Vê se o artigo existe */
            bool artigoExiste = await _context.Artigos
                .AnyAsync(a => a.ArtigoId == artigoId);

            if (!artigoExiste)
                return NotFound($"O artigo {artigoId} não existe.");

            /* Vê duplicados */
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

