using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommuniCare.Models;
using CommuniCare.DTOs;
using Microsoft.CodeAnalysis.Scripting;

namespace CommuniCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilizadoresController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public UtilizadoresController(CommuniCareContext context)
        {
            _context = context;
        }

        // GET: api/Utilizadors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Utilizador>>> GetUtilizadors()
        {
            return await _context.Utilizadores.ToListAsync();
        }

        // GET: api/Utilizadors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Utilizador>> GetUtilizador(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);

            if (utilizador == null)
            {
                return NotFound();
            }

            return utilizador;
        }

        // PUT: api/Utilizadors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUtilizador(int id, Utilizador utilizador)
        {
            if (id != utilizador.UtilizadorId)
            {
                return BadRequest();
            }

            _context.Entry(utilizador).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UtilizadorExists(id))
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

        // POST: api/Utilizadors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Utilizador>> PostUtilizador(Utilizador utilizador)
        {
            _context.Utilizadores.Add(utilizador);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUtilizador", new { id = utilizador.UtilizadorId }, utilizador);
        }

        // DELETE: api/Utilizadors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUtilizador(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null)
            {
                return NotFound();
            }

            _context.Utilizadores.Remove(utilizador);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UtilizadorDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool emailExiste = await _context.Contactos
                .AnyAsync(c => c.NumContacto == dto.Email);

            if (emailExiste)
            {
                return BadRequest("Já existe uma conta com este email.");
            }

            var novoUtilizador = new Utilizador
            {
                NomeUtilizador = dto.NomeUtilizador,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password), 
                NumCares = 0, 
                TipoUtilizadorId = 1 
            };

            _context.Utilizadores.Add(novoUtilizador);
            await _context.SaveChangesAsync();

            var contactoEmail = new Contacto
            {
                NumContacto = dto.Email,
                TipoContactoId = 1,
                UtilizadorId = novoUtilizador.UtilizadorId
            };

            _context.Contactos.Add(contactoEmail);
            await _context.SaveChangesAsync();

            return Ok("Conta criada com sucesso!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contacto = await _context.Contactos
                .Include(c => c.Utilizador)
                .FirstOrDefaultAsync(c => c.NumContacto == dto.Email && c.TipoContactoId == 1);

            if (contacto == null)
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            var utilizador = contacto.Utilizador;

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, utilizador.Password))
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            return Ok(new { Message = "Login bem-sucedido!", UtilizadorId = utilizador.UtilizadorId });
        }


        private bool UtilizadorExists(int id)
        {
            return _context.Utilizadores.Any(e => e.UtilizadorId == id);
        }


    }
}
