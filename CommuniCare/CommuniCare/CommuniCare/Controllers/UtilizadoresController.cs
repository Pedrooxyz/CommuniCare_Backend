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

        /// <summary>
        /// Obtém a lista de todos os utilizadores.
        /// </summary>
        /// <returns>Retorna uma lista de utilizadores ou 500 Internal Server Error em caso de falha.</returns>

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Utilizador>>> GetUtilizadors()
        {
            return await _context.Utilizadores.ToListAsync();
        }

        #region CONTROLLERS AUTOMÁTICOS
        /// <summary>
        /// Obtém os detalhes de um utilizador específico, baseado no seu ID.
        /// </summary>
        /// <param name="id">ID do utilizador a ser obtido.</param>
        /// <returns>Retorna o utilizador correspondente ao ID ou 404 Not Found se o utilizador não existir.</returns>

        //[HttpGet("{id}")]
        //public async Task<ActionResult<Utilizador>> GetUtilizador(int id)
        //{
        //    var utilizador = await _context.Utilizadores.FindAsync(id);

        //    if (utilizador == null)
        //    {
        //        return NotFound();
        //    }

        //    return utilizador;
        //}

        /// <summary>
        /// Atualiza os dados de um utilizador existente.
        /// </summary>
        /// <param name="id">ID do utilizador a ser atualizado.</param>
        /// <param name="utilizador">Objeto contendo os dados atualizados do utilizador.</param>
        /// <returns>Retorna 204 No Content se a atualização for bem-sucedida; retorna 400 Bad Request se os dados não coincidirem; 404 Not Found se o utilizador não existir ou 500 Internal Server Error em caso de falha.</returns>

        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutUtilizador(int id, Utilizador utilizador)
        //{
        //    if (id != utilizador.UtilizadorId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(utilizador).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!UtilizadorExists(id))
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

        /// <summary>
        /// Cria um novo utilizador.
        /// </summary>
        /// <param name="utilizador">Objeto contendo os dados do novo utilizador.</param>
        /// <returns>Retorna um status 201 Created com o utilizador criado, incluindo o URI do novo recurso; retorna 500 Internal Server Error em caso de falha.</returns>

        //[HttpPost]
        //public async Task<ActionResult<Utilizador>> PostUtilizador(Utilizador utilizador)
        //{
        //    _context.Utilizadores.Add(utilizador);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetUtilizador", new { id = utilizador.UtilizadorId }, utilizador);
        //}

        /// <summary>
        /// Deleta um utilizador específico baseado no seu ID.
        /// </summary>
        /// <param name="id">ID do utilizador a ser deletado.</param>
        /// <returns>Retorna 204 No Content se o utilizador for deletado com sucesso; retorna 404 Not Found se o utilizador não existir ou 500 Internal Server Error em caso de falha.</returns>

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteUtilizador(int id)
        //{
        //    var utilizador = await _context.Utilizadores.FindAsync(id);
        //    if (utilizador == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Utilizadores.Remove(utilizador);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        /// <summary>
        /// Regista um novo utilizador com os dados fornecidos.
        /// </summary>
        /// <param name="dto">Objeto DTO contendo os dados necessários para o registro.</param>
        /// <returns>Retorna um status 200 OK se a conta for criada com sucesso, aguardando aprovação de um administrador.</returns>
        #endregion

        /// <summary>
        /// Regista um novo administrador com os dados fornecidos.
        /// </summary>
        /// <param name="dto">Objeto DTO contendo os dados necessários para o registro de um administrador.</param>
        /// <returns>Retorna um status 200 OK se a conta de administrador for criada com sucesso, aguardando aprovação de outro administrador.</returns>

        [HttpPost("RegisterUtilizador")]
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

            if (string.IsNullOrWhiteSpace(dto.CPostal) || string.IsNullOrWhiteSpace(dto.Localidade))
            {
                return BadRequest("Código postal e localidade são obrigatórios.");
            }

            var cpExistente = await _context.Cps.FirstOrDefaultAsync(cp => cp.CPostal == dto.CPostal);
            if (cpExistente == null)
            {
                cpExistente = new Cp
                {
                    CPostal = dto.CPostal,
                    Localidade = dto.Localidade
                };
                _context.Cps.Add(cpExistente);
            }

            var morada = new Morada
            {
                Rua = dto.Rua,
                NumPorta = dto.NumPorta,
                CPostal = cpExistente.CPostal
            };
            _context.Morada.Add(morada);

            var novoUtilizador = new Utilizador
            {
                NomeUtilizador = dto.NomeUtilizador,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                NumCares = 0,
                TipoUtilizadorId = 1,
                Morada = morada, 
                EstadoUtilizador = EstadoUtilizador.Pendente
            };
            _context.Utilizadores.Add(novoUtilizador);

            var contactoEmail = new Contacto
            {
                NumContacto = dto.Email,
                TipoContactoId = 1, 
                Utilizador = novoUtilizador
            };
            _context.Contactos.Add(contactoEmail);

            var admins = await _context.Utilizadores
                .Where(u => u.TipoUtilizadorId == 2 && u.EstadoUtilizador == EstadoUtilizador.Ativo)
                .ToListAsync();

            var notificacoes = admins.Select(admin => new Notificacao
            {
                UtilizadorId = admin.UtilizadorId,
                Mensagem = $"Nova conta criada: {novoUtilizador.NomeUtilizador}. Aguardando aprovação.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                PedidoId = null,
                ItemId = null
            }).ToList();

            _context.Notificacaos.AddRange(notificacoes);

            await _context.SaveChangesAsync();

            var utilizadorComMorada = await _context.Utilizadores
                .Include(u => u.Morada)
                .FirstOrDefaultAsync(u => u.UtilizadorId == novoUtilizador.UtilizadorId);

            var resultado = new
            {
                utilizadorComMorada.UtilizadorId,
                utilizadorComMorada.NomeUtilizador,
                utilizadorComMorada.NumCares,
                utilizadorComMorada.MoradaId,
                Morada = new
                {
                    utilizadorComMorada.Morada?.Rua,
                    utilizadorComMorada.Morada?.NumPorta,
                    utilizadorComMorada.Morada?.CPostal
                },
                Message = "Conta criada com sucesso! Aguardando aprovação de um administrador."
            };

            return StatusCode(StatusCodes.Status201Created, resultado);
        }


        [HttpGet("ListarPendentes")]
        public async Task<IActionResult> ListarUtilizadoresPendentes()
        {
            var pendentes = await _context.Utilizadores
                .Where(u => u.EstadoUtilizador == EstadoUtilizador.Pendente)
                .Select(u => new
                {
                    u.UtilizadorId,
                    u.NomeUtilizador,
                    u.FotoUtil,
                    Morada = new
                    {
                        u.Morada.Rua,
                        u.Morada.NumPorta,
                        u.Morada.CPostal
                    }
                })
                .ToListAsync();

            return Ok(pendentes);
        }


        /// <summary>
        /// Obtém as informações do utilizador autenticado.
        /// </summary>
        /// <returns>Retorna as informações do utilizador autenticado ou 401 Unauthorized se o utilizador não for autenticado.</returns>

        [HttpGet("InfoUtilizador")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UtilizadorInfoDto>> GetCurrentUser()
        {

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (idClaim is null) return Unauthorized();

            if (!int.TryParse(idClaim.Value, out var userId))
                return Unauthorized();


            var utilizador = await _context.Utilizadores
                                      .AsNoTracking()
                                      .SingleOrDefaultAsync(u => u.UtilizadorId == userId);

            if (utilizador is null) return Unauthorized();


            var dto = new UtilizadorInfoDto
            {
                UtilizadorId = utilizador.UtilizadorId,
                NomeUtilizador = utilizador.NomeUtilizador,
                FotoUtil = utilizador.FotoUtil,
                NumCares = utilizador.NumCares,
                MoradaId = utilizador.MoradaId,
                TipoUtilizadorId = utilizador.TipoUtilizadorId
            };

            return Ok(dto);
        }


        /// <summary>
        /// Atualiza a foto do utilizador.
        /// </summary>
        /// <param name="dto">Objeto DTO contendo a nova URL da foto do utilizador.</param>
        /// <returns>Retorna 204 No Content se a foto for atualizada com sucesso; retorna 400 Bad Request se a URL da foto for inválida ou 401 Unauthorized se o utilizador não for autenticado.</returns>

        [HttpPut("EditarFoto")]
        [Authorize]
        public async Task<IActionResult> UpdateFoto(IFormFile foto)
        {
            if (foto == null || foto.Length == 0)
                return BadRequest("Nenhum ficheiro enviado.");

            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("sub") ??
                        User.FindFirstValue("uid") ??
                        User.FindFirstValue("id");

            if (!int.TryParse(idStr, out var userId))
                return Unauthorized();

            var utilizador = await _context.Utilizadores.SingleOrDefaultAsync(u => u.UtilizadorId == userId);
            if (utilizador is null)
                return Unauthorized();

            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            var fileName = $"{Guid.NewGuid()}_{foto.FileName}";
            var filePath = Path.Combine(uploadDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await foto.CopyToAsync(stream);
            }

            utilizador.FotoUtil = $"/uploads/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { fotoUtil = utilizador.FotoUtil });
        }

        /// <summary>
        /// Verifica se o utilizador é administrador.
        /// </summary>
        /// <returns>Retorna true se o utilizador for administrador (tipoClaim == "2"), caso contrário retorna false.</returns>
        [HttpGet("VerificarAdmin")]
        [Authorize]
        public IActionResult VerificarAdmin()
        {
            string? tipoClaim = User.FindFirstValue(ClaimTypes.Role);

            if (tipoClaim == "2")
            {
                return Ok(true); 
            }
            
            return Ok(false); 
        }
        
        #region Administrador

        /// <summary>
        /// Altera o tipo de um utilizador.
        /// </summary>
        /// <param name="id">ID do utilizador a ter o seu tipo alterado.</param>
        /// <returns>Retorna 200 Ok com mensagem de sucesso ou 404 Not Found se o utilizador não for encontrado.</returns>
        [Authorize]
        [HttpPut("/MudarTipoUtilizador/{id:int}")]
        public async Task<IActionResult> ChangeUserRole(int id, [FromBody] ChangeRoleRequest request, CancellationToken ct)
        {
            
            string? tipoClaim = User.FindFirstValue(ClaimTypes.Role);   

            if (tipoClaim != "2")                  
                return Unauthorized("Só administradores podem mudar o tipo de utilizador.");

            
            if (request.NewTipoUtilizadorId is not (1 or 2))
                return BadRequest("NewTipoUtilizadorId must be 1 or 2.");

            
            var target = await _context.Utilizadores
                                       .FirstOrDefaultAsync(u => u.UtilizadorId == id, ct);

            if (target is null)
                return NotFound($"No user with id {id}");

            
            if (target.TipoUtilizadorId == request.NewTipoUtilizadorId)
                return NoContent();

            
            target.TipoUtilizadorId = request.NewTipoUtilizadorId;
            target.SecurityStamp = Guid.NewGuid().ToString();   

            await _context.SaveChangesAsync(ct);

            return Ok(new
            {
                target.UtilizadorId,
                target.NomeUtilizador,
                target.TipoUtilizadorId
            });
        }

        [HttpPut("AprovarUtilizador-(admin)/{id}")]
        public async Task<IActionResult> AprovarUtilizador(int id)
        {

            var utilizador = await _context.Utilizadores.FindAsync(id);

            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            if (utilizador.EstadoUtilizador == EstadoUtilizador.Ativo)
                return BadRequest("Este utilizador já está ativo.");


            utilizador.EstadoUtilizador = EstadoUtilizador.Ativo;
            _context.Utilizadores.Update(utilizador);
            await _context.SaveChangesAsync();

            var notificacao = new Notificacao
            {
                UtilizadorId = utilizador.UtilizadorId,
                Mensagem = "A sua conta foi aprovada por um administrador. Já pode aceder à plataforma.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                PedidoId = null,
                ItemId = null
            };

            _context.Notificacaos.Add(notificacao);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Utilizador aprovado com sucesso."
            });
        }

        /// <summary>
        /// Rejeita um utilizador, desativando a sua conta.
        /// </summary>
        /// <param name="id">ID do utilizador a ser rejeitado.</param>
        /// <returns>Retorna 200 Ok com mensagem de sucesso ou 404 Not Found se o utilizador não for encontrado, ou 400 Bad Request se o utilizador já estiver inativo ou ativo.</returns>
        [Authorize]
        [HttpPut("RejeitarUtilizador-(admin)/{id}")]
        public async Task<IActionResult> RejeitarUtilizador(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);

            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            if (utilizador.EstadoUtilizador == EstadoUtilizador.Inativo)
                return BadRequest("Este utilizador já foi rejeitado.");

            if (utilizador.EstadoUtilizador == EstadoUtilizador.Ativo)
                return BadRequest("Não é possível rejeitar um utilizador que já está ativo.");

            utilizador.EstadoUtilizador = EstadoUtilizador.Inativo;
            _context.Utilizadores.Update(utilizador);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Utilizador rejeitado com sucesso."
            });
        }

        #endregion

        /// <summary>
        /// Completa o registo de um utilizador, associando uma morada a ele.
        /// </summary>
        /// <param name="dto">Objeto DTO contendo os dados da morada.</param>
        /// <returns>Retorna 200 Ok com mensagem de sucesso ou 400 Bad Request se o modelo for inválido, ou 404 Not Found se o utilizador não for encontrado.</returns>
        [HttpPost("CompletarRegisto")]
        [Authorize]
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

            var codigoPostal = await _context.Cps.FindAsync(dto.CPostal);
            if (codigoPostal == null)
            {

                codigoPostal = new Cp
                {
                    CPostal = dto.CPostal,
                    Localidade = dto.Localidade
                };

                _context.Cps.Add(codigoPostal);
                await _context.SaveChangesAsync();
            }

            moradaTemporaria.Rua = dto.Rua;
            moradaTemporaria.NumPorta = dto.NumPorta;
            moradaTemporaria.CPostal = dto.CPostal;

            _context.Morada.Update(moradaTemporaria);
            await _context.SaveChangesAsync();

            utilizador.MoradaId = moradaTemporaria.MoradaId;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Morada atualizada com sucesso! Registo completo." });
        }


        /// <summary>
        /// Realiza o login do utilizador, gerando um token JWT.
        /// </summary>
        /// <param name="dto">Objeto DTO contendo as credenciais do utilizador (email e password).</param>
        /// <returns>Retorna 200 Ok com o token de autenticação ou 400 Bad Request se as credenciais forem inválidas, ou 401 Unauthorized se a autenticação falhar.</returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var contacto = await _context.Contactos
                .Include(c => c.Utilizador)
                .FirstOrDefaultAsync(c => c.NumContacto == dto.Email && c.TipoContactoId == 1);

            if (contacto == null || contacto.Utilizador == null)
                return Unauthorized("Email ou password inválidos.");

            var utilizador = contacto.Utilizador;


            if (utilizador.EstadoUtilizador != EstadoUtilizador.Ativo)
                return Unauthorized("A sua conta ainda não está ativa.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, utilizador.Password))
                return Unauthorized("Email ou password inválidos.");

            var token = GerarToken(utilizador.UtilizadorId, dto.Email, utilizador.TipoUtilizadorId, utilizador);

            return Ok(new
            {
                Token = token,
                Message = "Login efetuado com sucesso"
            });
        }

        /// <summary>
        /// Gera um token JWT para autenticação do utilizador.
        /// </summary>
        /// <param name="utilizadorId">ID do utilizador.</param>
        /// <param name="email">Email do utilizador.</param>
        /// <param name="tipoUtilizadorId">ID do tipo de utilizador.</param>
        /// <returns>Retorna o token JWT gerado.</returns>
        private string GerarToken(int utilizadorId, string email, int tipoUtilizadorId, Utilizador user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString()),
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, tipoUtilizadorId.ToString()),
            new Claim("sstamp",      user.SecurityStamp) 
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

        /// <summary>
        /// Verifica se o utilizador com o ID fornecido existe.
        /// </summary>
        /// <param name="id">ID do utilizador a ser verificado.</param>
        /// <returns>Retorna true se o utilizador existir, false caso contrário.</returns>
        private bool UtilizadorExists(int id)
        {
            return _context.Utilizadores.Any(e => e.UtilizadorId == id);
        }


       /// <summary>
        /// Edita o nome, email e telemóvel do utilizador autenticado.
        /// </summary>
        /// <param name="dto">Objeto DTO com nome, email e telemóvel.</param>
        /// <returns>200 OK se sucesso, 404 se utilizador não for encontrado.</returns>
        [HttpPut("EditarPerfil")]
        [Authorize]
        public async Task<IActionResult> EditarPerfil([FromBody] EditarPerfilDTO dto)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var utilizador = await _context.Utilizadores
                .Include(u => u.Contactos)
                .FirstOrDefaultAsync(u => u.UtilizadorId == utilizadorId);

            if (utilizador == null) return NotFound("Utilizador não encontrado.");

            if (!string.IsNullOrEmpty(dto.Nome))
                utilizador.NomeUtilizador = dto.Nome;

            if (!string.IsNullOrEmpty(dto.Email))
            {
                var contactoEmail = utilizador.Contactos.FirstOrDefault(c => c.TipoContactoId == 1);
                if (contactoEmail != null)
                {
                    contactoEmail.NumContacto = dto.Email;
                }
                else
                {
                    utilizador.Contactos.Add(new Contacto
                    {
                        TipoContactoId = 1,
                        NumContacto = dto.Email,
                        UtilizadorId = utilizadorId
                    });
                }
            }

            if (!string.IsNullOrEmpty(dto.Telemovel))
            {
                var contactoTelemovel = utilizador.Contactos.FirstOrDefault(c => c.TipoContactoId == 2);
                if (contactoTelemovel != null)
                {
                    contactoTelemovel.NumContacto = dto.Telemovel;
                }
                else
                {
                    utilizador.Contactos.Add(new Contacto
                    {
                        TipoContactoId = 2,
                        NumContacto = dto.Telemovel,
                        UtilizadorId = utilizadorId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Perfil atualizado com sucesso.");
        }

        /// <summary>
        /// Obtém o saldo de cares do utilizador autenticado.
        /// </summary>
        /// <returns>Retorna 200 Ok com o saldo de cares do utilizador ou 404 Not Found se o utilizador não for encontrado.</returns>
        [HttpGet("ObterSaldo")]
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

        /// <summary>
        /// Apaga a conta do utilizador, tornando-a inativa e anonimizada.
        /// </summary>
        /// <param name="dto">Objeto DTO contendo a senha para confirmação.</param>
        /// <returns>Retorna 200 Ok com mensagem de sucesso ou 400 Bad Request se a senha for incorreta.</returns>
        [HttpDelete("ApagarConta")]
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

            utilizador.EstadoUtilizador = EstadoUtilizador.Inativo;

            utilizador.NomeUtilizador = "Utilizador Removido";
            utilizador.FotoUtil = null;
            utilizador.Password = null;

            var relacoes = await _context.ItemEmprestimoUtilizadores
                .Where(r => r.UtilizadorId == utilizadorId)
                .ToListAsync();

            foreach (var r in relacoes)
                r.UtilizadorId = null;

            if (utilizador.Contactos != null)
                _context.Contactos.RemoveRange(utilizador.Contactos);

            if (utilizador.Morada != null)
                _context.Morada.Remove(utilizador.Morada);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Conta desativada com sucesso." });
        }

        #region Reset Password

        /// <summary>
        /// Solicita o envio de um e-mail para recuperação de senha, com um link para redefinir a senha.
        /// </summary>
        /// <param name="dto">Objeto DTO contendo o e-mail do utilizador para enviar o link de recuperação.</param>
        /// <returns>Retorna 200 Ok se o e-mail de recuperação for enviado com sucesso, ou 400 Bad Request se o e-mail não for fornecido, ou 404 Not Found se o utilizador não for encontrado.</returns>
        [HttpPost("RecuperarSenha")]
        public async Task<IActionResult> RecuperarSenha([FromBody] RecuperarSenhaDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Email))
            {
                return BadRequest("E-mail não fornecido.");
            }


            var utilizador = await _context.Utilizadores
                .Include(u => u.Contactos)
                .FirstOrDefaultAsync(u => u.Contactos.Any(c => c.NumContacto == dto.Email));

            if (utilizador == null)
            {
                return NotFound("Utilizador não encontrado.");
            }


            var tokenRecuperacao = GerarTokenRecuperacaoSenha(utilizador.UtilizadorId);
            var resetLink = $"http://localhost:3000/resetpassword?token={tokenRecuperacao}";


            var emailService = new EmailService(_configuration);
            await emailService.SendPasswordResetEmail(dto.Email, resetLink);

            return Ok("E-mail de recuperação enviado.");
        }

        /// <summary>
        /// Gera um token JWT para a recuperação de senha do utilizador.
        /// </summary>
        /// <param name="utilizadorId">ID do utilizador para o qual o token será gerado.</param>
        /// <returns>Retorna o token JWT gerado para recuperação de senha.</returns>
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

        /// <summary>
        /// Redefine a senha do utilizador utilizando o token de recuperação e a nova senha fornecida.
        /// </summary>
        /// <param name="token">Token de recuperação de senha gerado previamente.</param>
        /// <param name="dto">Objeto DTO contendo a nova senha do utilizador.</param>
        /// <returns>Retorna 200 Ok se a senha for redefinida com sucesso, ou 400 Bad Request
        [HttpPost("ResetarSenha")]
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

        /// <summary>
        /// Adiciona uma quantidade de cares ao utilizador especificado.
        /// </summary>
        /// <param name="dto">Objeto DTO contendo o ID do utilizador e a quantidade de cares a ser adicionada.</param>
        /// <returns>Retorna 200 Ok com a mensagem de sucesso e o saldo de cares atualizado, ou 400 Bad Request se a quantidade de cares for inválida, ou 404 Not Found se o utilizador não for encontrado.</returns>
        [HttpPost("AdicionarCares-(teste)")]
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
