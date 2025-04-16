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
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;


//É NECESSÁRIO COLOCAR O ESTADO DO UTILIZADOR
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
            // Garantir que existe um Código Postal padrão
            var codigoPostalPadrao = await _context.Cps.FirstOrDefaultAsync(cp => cp.CPostal == "0000-000");
            if (codigoPostalPadrao == null)
            {
                codigoPostalPadrao = new Cp
                {
                    CPostal = "0000-000",
                    Localidade = "000000"
                };
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
            var token = GerarToken(novoUtilizador.UtilizadorId, dto.Email, novoUtilizador.TipoUtilizadorId);

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

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int utilizadorId))
            {
                return Unauthorized("Token inválido ou utilizador não autenticado.");
            }


            var utilizador = await _context.Utilizadores
                .Include(u => u.Morada)
                .FirstOrDefaultAsync(u => u.UtilizadorId == utilizadorId);

            if (utilizador == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            var moradaTemporaria = utilizador.Morada;

            // Verificar se o código postal fornecido já existe
            var codigoPostal = await _context.Cps.FindAsync(dto.CPostal);
            if (codigoPostal == null)
            {
                // Criar novo código postal com a localidade fornecida
                codigoPostal = new Cp
                {
                    CPostal = dto.CPostal,
                    Localidade = dto.Localidade
                };

                _context.Cps.Add(codigoPostal);
                await _context.SaveChangesAsync();
            }

            // Atualiza a morada
            moradaTemporaria.Rua = dto.Rua;
            moradaTemporaria.NumPorta = dto.NumPorta;
            moradaTemporaria.CPostal = dto.CPostal;

            _context.Morada.Update(moradaTemporaria);
            await _context.SaveChangesAsync();

            // Opcional: manter referência correta no utilizador
            utilizador.MoradaId = moradaTemporaria.MoradaId;
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
            var token = GerarToken(utilizador.UtilizadorId, dto.Email, utilizador.TipoUtilizadorId);

            return Ok(new
            {
                Token = token,
                Message = "Login efetuado com sucesso"
            });
        }

        private string GerarToken(int utilizadorId, string email, int tipoUtilizadorId)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString()),
        new Claim(ClaimTypes.Name, email),
        new Claim(ClaimTypes.Role, tipoUtilizadorId.ToString())
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


        [HttpPut("editar-perfil")]
        [Authorize]
        public async Task<IActionResult> EditarPerfil([FromBody] EditarPerfilDTO dto)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var utilizador = await _context.Utilizadores
                .Include(u => u.Contactos)
                .Include(u => u.Morada)
                .FirstOrDefaultAsync(u => u.UtilizadorId == utilizadorId);

            if (utilizador == null) return NotFound("Utilizador não encontrado.");

            if (!string.IsNullOrEmpty(dto.Nome))
                utilizador.NomeUtilizador = dto.Nome;

            // Atualiza contactos individualmente
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var contactoEmail = utilizador.Contactos.FirstOrDefault(c => c.TipoContactoId == 1);
                if (contactoEmail != null)
                    contactoEmail.NumContacto = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.Telemovel))
            {
                var contactoTele = utilizador.Contactos.FirstOrDefault(c => c.TipoContactoId == 2);
                if (contactoTele != null)
                    contactoTele.NumContacto = dto.Telemovel;
            }

            // Atualiza morada se existir
            if (utilizador.Morada != null)
            {
                if (!string.IsNullOrEmpty(dto.Rua)) utilizador.Morada.Rua = dto.Rua;
                if (dto.NumPorta == null) utilizador.Morada.NumPorta = dto.NumPorta;

                if (!string.IsNullOrEmpty(dto.CPostal))
                {
                    var cp = await _context.Cps.FindAsync(dto.CPostal);
                    if (cp == null)
                    {
                        cp = new Cp { CPostal = dto.CPostal, Localidade = dto.Localidade ?? "" };
                        _context.Cps.Add(cp);
                        await _context.SaveChangesAsync();
                    }
                    utilizador.Morada.CPostal = cp.CPostal;
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Perfil atualizado com sucesso.");
        }

        [HttpGet("saldo")]
        [Authorize]
        public async Task<IActionResult> ObterSaldo()
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.UtilizadorId == utilizadorId);

            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            return Ok(new { Saldo = utilizador.NumCares });
        }

        [HttpDelete("apagar-conta")]
        [Authorize]
        public async Task<IActionResult> ApagarConta([FromBody] ConfirmarPasswordDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var utilizador = await _context.Utilizadores
                .Include(u => u.Morada)
                .Include(u => u.Contactos)
                .FirstOrDefaultAsync(u => u.UtilizadorId == utilizadorId);

            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, utilizador.Password))
                return Unauthorized("Password incorreta.");

            if (utilizador.Contactos != null)
                _context.Contactos.RemoveRange(utilizador.Contactos);

            if (utilizador.Morada != null)
                _context.Morada.Remove(utilizador.Morada);

            _context.Utilizadores.Remove(utilizador);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Conta apagada com sucesso." });
        }

        #region Reset Password

        [HttpPost("recuperar-senha")]
        public async Task<IActionResult> RecuperarSenha([FromBody] RecuperarSenhaDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Email))
            {
                return BadRequest("E-mail não fornecido.");
            }

            // Verificar se o utilizador existe com o e-mail fornecido
            var utilizador = await _context.Utilizadores
                .Include(u => u.Contactos)
                .FirstOrDefaultAsync(u => u.Contactos.Any(c => c.NumContacto == dto.Email));

            if (utilizador == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            // Gerar o token de recuperação
            var tokenRecuperacao = GerarTokenRecuperacaoSenha(utilizador.UtilizadorId);
            var resetLink = $"{Request.Scheme}://{Request.Host}/api/utilizadores/resetar-senha?token={tokenRecuperacao}";

            // Enviar o e-mail de recuperação
            var emailService = new EmailService(_configuration);
            await emailService.SendPasswordResetEmail(dto.Email, resetLink);

            return Ok("E-mail de recuperação enviado.");
        }

        private string GerarTokenRecuperacaoSenha(int utilizadorId)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]);
            var symmetricKey = new SymmetricSecurityKey(key);

            var token = new JwtSecurityToken(
                issuer: "CommuniCare",
                audience: "CommuniCareUsers",
                claims: new[] { new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString()) },
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost("resetar-senha")]
        public async Task<IActionResult> ResetarSenha([FromQuery] string token, [FromBody] NovaSenhaDTO dto)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(dto.NovaSenha))
            {
                return BadRequest("Token ou nova senha não fornecidos.");
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]);
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "CommuniCare",
                    ValidateAudience = true,
                    ValidAudience = "CommuniCareUsers",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = handler.ValidateToken(token, parameters, out _);
                var utilizadorId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (utilizadorId == null)
                    return Unauthorized("Token inválido.");

                var utilizador = await _context.Utilizadores.FindAsync(int.Parse(utilizadorId));
                if (utilizador == null)
                    return NotFound("Utilizador não encontrado.");

                utilizador.Password = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha);
                _context.Utilizadores.Update(utilizador);
                await _context.SaveChangesAsync();

                return Ok("Senha redefinida com sucesso.");
            }
            catch (SecurityTokenException)
            {
                return Unauthorized("Token inválido ou expirado.");
            }
        }

        #endregion

        #region TESTE 

        [HttpPost("adicionar-cares")]
        public async Task<IActionResult> AdicionarCares([FromBody] AdicionarCaresDTO dto)
        {
            var utilizador = await _context.Utilizadores.FindAsync(dto.UtilizadorId);

            if (utilizador == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            if (dto.Quantidade <= 0)
            {
                return BadRequest("A quantidade de cares deve ser maior que zero.");
            }

            utilizador.NumCares = (utilizador.NumCares ?? 0) + dto.Quantidade;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Cares adicionados com sucesso.", totalCares = utilizador.NumCares });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar cares: {ex.Message}");
            }
        }


        #endregion

    }
}
