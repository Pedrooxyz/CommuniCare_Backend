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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CommuniCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilizadoresController : ControllerBase
    {
        private readonly CommuniCareContext _context;
        private readonly IConfiguration _configuration;

        public UtilizadoresController(CommuniCareContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            // Garantir que existe um Código Postal padrão
            var codigoPostalPadrao = await _context.Cps.FirstOrDefaultAsync(cp => cp.Localidade == "000000"); // O DescCP como string
            if (codigoPostalPadrao == null)
            {
                codigoPostalPadrao = new Cp { Localidade = "000000" };
                _context.Cps.Add(codigoPostalPadrao);
                await _context.SaveChangesAsync();
            }

            // Criar uma morada temporária associada a esse Código Postal
            var moradaTemporaria = new Morada
            {
                Rua = "A definir",
                NumPorta = null,
                CPostal = codigoPostalPadrao.CPostal // Associar ao Código Postal padrão
            };

            _context.Morada.Add(moradaTemporaria);
            await _context.SaveChangesAsync();

            // Criar o utilizador associado à morada temporária
            var novoUtilizador = new Utilizador
            {
                NomeUtilizador = dto.NomeUtilizador,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                NumCares = 0,
                TipoUtilizadorId = 1,
                MoradaId = moradaTemporaria.MoradaId // Associar à morada temporária
            };

            _context.Utilizadores.Add(novoUtilizador);
            await _context.SaveChangesAsync();

            // Criar o contacto do utilizador
            var contactoEmail = new Contacto
            {
                NumContacto = dto.Email,
                TipoContactoId = 1,
                UtilizadorId = novoUtilizador.UtilizadorId
            };

            _context.Contactos.Add(contactoEmail);
            await _context.SaveChangesAsync();

            // Gerar token para o utilizador
            var token = GerarToken(novoUtilizador.UtilizadorId, dto.Email);

            // Retornar o token JWT
            return Ok(new
            {
                Message = "Conta criada com sucesso!",
                Token = token
            });
        }


        [HttpPost("completar-registo")]
        public async Task<IActionResult> CompletarRegisto([FromBody] MoradaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obter o UtilizadorId a partir do JWT (do token enviado no cabeçalho Authorization)
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); // Recupera o 'UtilizadorId' a partir das claims

            // Verifica se o utilizador existe
            var utilizador = await _context.Utilizadores
                .Include(u => u.Morada) // Incluir a morada associada
                .FirstOrDefaultAsync(u => u.UtilizadorId == utilizadorId);

            if (utilizador == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            // Verifica se o utilizador tem uma morada temporária associada
            var moradaTemporaria = utilizador.Morada;

            if (moradaTemporaria == null || moradaTemporaria.Rua == "A definir")
            {
                return BadRequest("Morada temporária não encontrada ou ainda não foi definida.");
            }

            // Atualiza a morada com os novos dados fornecidos pelo utilizador
            moradaTemporaria.Rua = dto.Rua;
            moradaTemporaria.NumPorta = dto.NumPorta;
            moradaTemporaria.CPostal = dto.CPostal; // Novo Código Postal

            // Salva as alterações no banco de dados
            _context.Morada.Update(moradaTemporaria);
            await _context.SaveChangesAsync();

            // Atualiza o utilizador (opcional, dependendo da lógica que queres implementar)
            utilizador.MoradaId = moradaTemporaria.MoradaId;  // A morada já está associada ao utilizador, então não é necessário modificar

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Morada atualizada com sucesso! Registo completo." });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Procurar contacto com email
            var contacto = await _context.Contactos
                .Include(c => c.Utilizador)
                .FirstOrDefaultAsync(c => c.NumContacto == dto.Email && c.TipoContactoId == 1);

            if (contacto == null || contacto.Utilizador == null)
                return Unauthorized("Email ou password inválidos.");

            var utilizador = contacto.Utilizador;

            // Verificar password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, utilizador.Password))
                return Unauthorized("Email ou password inválidos.");

            // Gerar token
            var token = GerarToken(utilizador.UtilizadorId, dto.Email);

            return Ok(new
            {
                Token = token,
                Message = "Login efetuado com sucesso"
            });
        }

        private string GerarToken(int utilizadorId, string email)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString()),
        new Claim(ClaimTypes.Name, email),
        new Claim(ClaimTypes.Role, "Utilizador")
    };

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }






        private bool UtilizadorExists(int id)
        {
            return _context.Utilizadores.Any(e => e.UtilizadorId == id);
        }


    }
}
