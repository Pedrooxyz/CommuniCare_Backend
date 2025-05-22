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
    public class PedidosAjudaController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        /// <summary>
        /// Construtor da classe <see cref="PedidosAjudaController"/>.
        /// </summary>
        /// <param name="context">O contexto de dados do aplicativo.</param>
        public PedidosAjudaController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todos os pedidos de ajuda.
        /// </summary>
        /// <returns>Uma lista de pedidos de ajuda.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoAjuda>>> GetPedidoAjuda()
        {
            return await _context.PedidosAjuda.ToListAsync();
        }

        #region CONTROLLERS AUTOMÁTICOS
        /// <summary>
        /// Obtém um pedido de ajuda específico pelo ID.
        /// </summary>
        /// <param name="id">O ID do pedido de ajuda.</param>
        /// <returns>O pedido de ajuda correspondente ao ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoAjuda>> GetPedidoAjuda(int id)
        {
            var pedidoAjuda = await _context.PedidosAjuda.FindAsync(id);

            if (pedidoAjuda == null)
            {
                return NotFound();
            }

            return pedidoAjuda;
        }

        ///// <summary>
        ///// Atualiza um pedido de ajuda específico.
        ///// </summary>
        ///// <param name="id">O ID do pedido de ajuda a ser atualizado.</param>
        ///// <param name="pedidoAjuda">O novo pedido de ajuda.</param>
        ///// <returns>Status da operação.</returns>

        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutPedidoAjuda(int id, PedidoAjuda pedidoAjuda)
        //{
        //    if (id != pedidoAjuda.PedidoId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(pedidoAjuda).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!PedidoAjudaExists(id))
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

        ///// <summary>
        ///// Cria um novo pedido de ajuda.
        ///// </summary>
        ///// <param name="pedidoAjuda">Os dados do novo pedido de ajuda.</param>
        ///// <returns>O pedido de ajuda criado.</returns>
        //[HttpPost]
        //public async Task<ActionResult<PedidoAjuda>> PostPedidoAjuda(PedidoAjuda pedidoAjuda)
        //{
        //    _context.PedidosAjuda.Add(pedidoAjuda);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetPedidoAjuda", new { id = pedidoAjuda.PedidoId }, pedidoAjuda);
        //}

        ///// <summary>
        ///// Remove um pedido de ajuda específico pelo ID.
        ///// </summary>
        ///// <param name="id">O ID do pedido de ajuda a ser removido.</param>
        ///// <returns>Status da operação.</returns>
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeletePedidoAjuda(int id)
        //{
        //    var pedidoAjuda = await _context.PedidosAjuda.FindAsync(id);
        //    if (pedidoAjuda == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.PedidosAjuda.Remove(pedidoAjuda);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        #endregion

        private bool PedidoAjudaExists(int id)
        {
            return _context.PedidosAjuda.Any(e => e.PedidoId == id);
        }

        /// <summary>
        /// Cria um novo pedido de ajuda.
        /// </summary>
        /// <param name="pedidoData">Os dados do pedido de ajuda a ser criado.</param>
        /// <returns>Status da operação.</returns>
        [HttpPost("Pedir")]
        [Authorize]
        public async Task<IActionResult> CriarPedidoAjuda([FromBody] PedidoAjudaDTO pedidoData)
        {
            if (pedidoData == null ||
                string.IsNullOrWhiteSpace(pedidoData.DescPedido) ||
                pedidoData.NHoras <= 0 ||
                pedidoData.NPessoas <= 0)
            {
                return BadRequest("Dados inválidos.");
            }

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

            var pedido = new PedidoAjuda
            {
                Titulo = pedidoData.Titulo,
                DescPedido = pedidoData.DescPedido,
                HorarioAjuda = pedidoData.HorarioAjuda,
                NHoras = pedidoData.NHoras,
                NPessoas = pedidoData.NPessoas,
                UtilizadorId = utilizadorId,
                Estado = EstadoPedido.Pendente,
                FotografiaPA = pedidoData.FotografiaPA,
                RecompensaCares = pedidoData.NHoras * 50
            };

            _context.PedidosAjuda.Add(pedido);
            await _context.SaveChangesAsync();

            var administradores = await _context.Utilizadores
                .Where(u => u.TipoUtilizadorId == 2)
                .ToListAsync();

            var notificacoes = administradores.Select(admin => new Notificacao
            {
                Mensagem = $"Foi criado um novo pedido de ajuda por {utilizador.NomeUtilizador}.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                PedidoId = pedido.PedidoId,
                UtilizadorId = admin.UtilizadorId,
                ItemId = null
            }).ToList();

            _context.Notificacaos.AddRange(notificacoes);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensagem = "Pedido de ajuda criado com sucesso. Notificações enviadas aos administradores."
            });
        }


        /// <summary>
        /// Registra um voluntário para um pedido de ajuda.
        /// </summary>
        /// <param name="pedidoId">O ID do pedido de ajuda.</param>
        /// <returns>Status da operação.</returns>
        [HttpPost("{pedidoId}/Voluntariar")]
        [Authorize]
        public async Task<IActionResult> Voluntariar(int pedidoId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var pedido = await _context.PedidosAjuda
                .Include(p => p.Voluntariados)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

            if (pedido == null || pedido.Estado != EstadoPedido.Aberto)
            {
                return BadRequest("Pedido não encontrado ou já fechado.");
            }

            if (pedido.NPessoas.HasValue && pedido.Voluntariados.Count >= pedido.NPessoas.Value)
            {
                return BadRequest("Número máximo de voluntários já atingido para este pedido.");
            }

            bool jaVoluntariado = pedido.Voluntariados.Any(v => v.UtilizadorId == utilizadorId);
            if (jaVoluntariado)
            {
                return BadRequest("Utilizador já se voluntariou para este pedido.");
            }

            var voluntariado = new Voluntariado
            {
                PedidoId = pedidoId,
                UtilizadorId = utilizadorId,
                Estado = EstadoVoluntariado.Pendente
            };

            _context.Voluntariados.Add(voluntariado);

            var admins = await _context.Utilizadores
                .Where(u => u.TipoUtilizadorId == 2)
                .ToListAsync();

            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);

            foreach (var admin in admins)
            {
                var notificacao = new Notificacao
                {
                    Mensagem = $"{utilizador?.NomeUtilizador ?? "Um utilizador"} voluntariou-se para o pedido #{pedidoId}.",
                    Lida = 0,
                    DataMensagem = DateTime.Now,
                    PedidoId = pedidoId,
                    UtilizadorId = admin.UtilizadorId,
                    ItemId = null
                };

                _context.Notificacaos.Add(notificacao);
            }

            await _context.SaveChangesAsync();

            return Ok("Pedido de voluntariado registado com sucesso. Aguardando aprovação do administrador.");
        }

        #region Administrador

        /// <summary>
        /// Rejeita um pedido de ajuda.
        /// </summary>
        /// <param name="pedidoId">O ID do pedido de ajuda a ser rejeitado.</param>
        /// <returns>Status da operação.</returns>
        [HttpPost("{pedidoId}/RejeitarPedido-(admin)")]
        [Authorize]
        public async Task<IActionResult> RejeitarPedidoAjuda(int pedidoId)
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
                return Forbid("Apenas administradores podem validar pedidos de ajuda.");
            }

            var pedido = await _context.PedidosAjuda
                .Include(p => p.Utilizador)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

            if (pedido == null)
            {
                return NotFound("Pedido de ajuda não encontrado.");
            }

            if (pedido.Estado != EstadoPedido.Pendente)
            {
                return BadRequest("Este pedido já foi validado ou está em progresso/concluído.");
            }

            pedido.Estado = EstadoPedido.Rejeitado;

            var notificacao = new Notificacao
            {
                Mensagem = $"O teu pedido de ajuda #{pedido.PedidoId} foi rejeitado por um administrador por ser considerado inválido.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                PedidoId = pedido.PedidoId,
                UtilizadorId = pedido.UtilizadorId,
                ItemId = null
            };
            _context.Notificacaos.Add(notificacao);

            await _context.SaveChangesAsync();

            return Ok("Pedido de ajuda rejeitado com sucesso.");
        }

        /// <summary>
        /// Valida um pedido de ajuda, alterando seu estado para "Aberto" e notificando outros utilizadores.
        /// </summary>
        /// <param name="pedidoId">ID do pedido de ajuda a ser validado.</param>
        /// <returns>Retorna um status 200 OK se o pedido for validado com sucesso; retorna 401 Unauthorized, 403 Forbidden, 404 Not Found ou 400 Bad Request em caso de erro.</returns>

        [HttpPost("{pedidoId}/ValidarPedido-(admin)")]
        [Authorize]
        public async Task<IActionResult> ValidarPedidoAjuda(int pedidoId)
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
                return Forbid("Apenas administradores podem validar pedidos de ajuda.");
            }

            var pedido = await _context.PedidosAjuda.FindAsync(pedidoId);
            if (pedido == null)
            {
                return NotFound("Pedido de ajuda não encontrado.");
            }

            if (pedido.Estado != EstadoPedido.Pendente)
            {
                return BadRequest("Este pedido já foi validado ou está em progresso/concluído.");
            }

            pedido.Estado = EstadoPedido.Aberto;

            await _context.SaveChangesAsync();

            var outrosUtilizadores = await _context.Utilizadores
                .Where(u => u.UtilizadorId != utilizadorId)
                .ToListAsync();

            var notificacoes = outrosUtilizadores.Select(u => new Notificacao
            {
                Mensagem = $"O utilizador {utilizador.NomeUtilizador} criou um novo pedido de ajuda.",
                Lida = 0,
                DataMensagem = DateTime.Now,
                PedidoId = pedido.PedidoId,
                UtilizadorId = pedido.UtilizadorId,
                ItemId = null
            }).ToList();

            _context.Notificacaos.AddRange(notificacoes);
            await _context.SaveChangesAsync();

            return Ok("Pedido de ajuda validado com sucesso e colocado como 'Aberto'.");
        }

        /// <summary>
        /// Marca um pedido de ajuda como "Concluído" e notifica os administradores para validação.
        /// </summary>
        /// <param name="pedidoId">ID do pedido de ajuda a ser concluído.</param>
        /// <returns>Retorna um status 200 OK se o pedido for concluído com sucesso e os administradores forem notificados; retorna 401 Unauthorized, 403 Forbidden, 404 Not Found ou 400 Bad Request em caso de erro.</returns>

        [HttpPost("ConcluirPedido/{pedidoId}")]
        [Authorize]
        public async Task<IActionResult> ConcluirPedidoAjuda(int pedidoId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var pedido = await _context.PedidosAjuda
                .Include(p => p.Utilizador)
                .Include(p => p.Voluntariados)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

            if (pedido == null)
            {
                return NotFound("Pedido de ajuda não encontrado.");
            }

            if (pedido.UtilizadorId != utilizadorId)
            {
                return Forbid("Apenas o requisitante do pedido pode marcá-lo como concluído.");
            }

            if (pedido.Estado != EstadoPedido.EmProgresso)
            {
                return BadRequest("O pedido não está em progresso ou já foi concluído.");
            }

            pedido.Estado = EstadoPedido.Concluido;

            var admins = await _context.Utilizadores
                .Where(u => u.TipoUtilizadorId == 2)
                .ToListAsync();

            foreach (var admin in admins)
            {
                var notificacaoAdmin = new Notificacao
                {
                    Mensagem = $"O utilizador {pedido.Utilizador.NomeUtilizador} marcou o pedido #{pedido.PedidoId} como concluído. Valida esta conclusão.",
                    Lida = 0,
                    DataMensagem = DateTime.Now,
                    PedidoId = pedido.PedidoId,
                    UtilizadorId = admin.UtilizadorId,
                    ItemId = null
                };
                _context.Notificacaos.Add(notificacaoAdmin);
            }

            await _context.SaveChangesAsync();

            return Ok("O pedido foi marcado como concluído. Os administradores foram notificados para validar a conclusão.");
        }

        /// <summary>
        /// Valida a conclusão de um pedido de ajuda, atribuindo a recompensa ao utilizador e registrando a transação.
        /// </summary>
        /// <param name="pedidoId">ID do pedido de ajuda a ter sua conclusão validada.</param>
        /// <returns>Retorna um status 200 OK se a conclusão for validada com sucesso e a recompensa for atribuída; retorna 401 Unauthorized, 403 Forbidden, 404 Not Found ou 400 Bad Request em caso de erro.</returns>

        [HttpPost("ValidarConclusao-(admin)/{pedidoId}")]
        [Authorize]
        public async Task<IActionResult> ValidarConclusaoPedidoAjuda(int pedidoId)
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
                return Forbid("Apenas administradores podem validar pedidos de ajuda.");
            }

            var pedido = await _context.PedidosAjuda
                .Include(p => p.Utilizador)
                .Include(p => p.Voluntariados)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

            if (pedido == null)
            {
                return NotFound("Pedido de ajuda não encontrado.");
            }

            if (pedido.Estado != EstadoPedido.Concluido)
            {
                return BadRequest("O pedido não ainda não foi concluído.");
            }

            var recetor = pedido.Utilizador;

            if (recetor == null)
            {
                return BadRequest("Não foi possível determinar o recetor do pedido.");
            }

            int recompensa = pedido.RecompensaCares ?? 0;

            recetor.NumCares += recompensa;

            var transacao = new Transacao
            {
                DataTransacao = DateTime.UtcNow,
                Quantidade = recompensa
            };

            var transacaoAjuda = new TransacaoAjuda
            {
                RecetorTran = recetor.UtilizadorId,
                Transacao = transacao,
                PedidoAjuda = new List<PedidoAjuda> { pedido }
            };

            _context.Transacoes.Add(transacao);
            _context.TransacaoAjuda.Add(transacaoAjuda);

            var voluntariado = pedido.Voluntariados.FirstOrDefault();
            if (voluntariado != null)
            {
                var notificacaoVoluntario = new Notificacao
                {
                    Mensagem = $"A transação foi efetuada com sucesso para o pedido #{pedido.PedidoId}. Obrigado pela tua ajuda!",
                    Lida = 0,
                    DataMensagem = DateTime.Now,
                    PedidoId = pedido.PedidoId,
                    UtilizadorId = voluntariado.UtilizadorId,
                    ItemId = null
                };
                _context.Notificacaos.Add(notificacaoVoluntario);
            }

            await _context.SaveChangesAsync();

            return Ok("Pedido de ajuda concluído com sucesso. Recompensa atribuída, transação registada e notificação enviada.");
        }

        #endregion

        /// <summary>
        /// Obtém todos os pedidos de ajuda disponíveis para o utilizador, exceto os pedidos criados pelo próprio utilizador.
        /// </summary>
        /// <returns>Retorna uma lista de pedidos de ajuda disponíveis ou 401 Unauthorized se o utilizador não estiver autenticado.</returns>

        [Authorize]
        [HttpGet("PedidosDisponiveis")]
        public async Task<ActionResult<IEnumerable<PedidoAjuda>>> GetPedidosAjudaDisponiveis()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var pedidosDisponiveis = await _context.PedidosAjuda
                .Where(p => p.Estado == EstadoPedido.Aberto && p.UtilizadorId != utilizadorId)
                .ToListAsync();

            return Ok(pedidosDisponiveis);
        }

        /// <summary>
        /// Obtém todos os pedidos de ajuda criados pelo utilizador autenticado.
        /// </summary>
        /// <returns>Retorna uma lista de pedidos de ajuda do utilizador ou 401 Unauthorized se o utilizador não estiver autenticado.</returns>

        [Authorize]
        [HttpGet("MeusPedidos")]
        public async Task<ActionResult<IEnumerable<PedidoAjuda>>> GetMeusPedidosAjuda()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var meusPedidos = await _context.PedidosAjuda
                .Where(p => p.UtilizadorId == utilizadorId)
                .ToListAsync();

            return Ok(meusPedidos);
        }

        /// <summary>
        /// Retorna todos os pedidos de ajuda que requerem ações administrativas.
        /// Apenas administradores podem aceder a este endpoint.
        /// </summary>
        /// <returns>Lista de pedidos pendentes com detalhes completas para administração.</returns>
        [HttpGet("Admin/Pendentes")]
        [Authorize]
        public async Task<IActionResult> ObterPedidosPendentesAdmin()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int adminId = int.Parse(userIdClaim.Value);

            var admin = await _context.Utilizadores.FindAsync(adminId);
            if (admin == null || admin.TipoUtilizadorId != 2)
                return Forbid("Apenas administradores podem aceder a esta informação.");

            var pedidos = await _context.PedidosAjuda
                .Include(p => p.Voluntariados)
                .Include(p => p.Transacao)
                .Where(p => p.Estado == EstadoPedido.Pendente)
                .ToListAsync();

            var pedidosDTO = pedidos.Select(p => new PedidoPendenteDTO
            {
                PedidoId = p.PedidoId,
                Titulo = p.Titulo,
                Descricao = p.DescPedido,
                DataCriacao = p.HorarioAjuda,
                NumeroVoluntarios = p.Voluntariados.Count,
                Transacao = p.Transacao == null ? null : new TransacaoDTO
                {
                    Id = p.Transacao.TransacaoId,
                    DataTransacao = p.Transacao.Transacao.DataTransacao ?? DateTime.MinValue
                }
            }).ToList();

            return Ok(pedidosDTO);
        }


    /// <summary>
    /// Retorna todos os pedidos que requerem ações do administrador.
    /// </summary>
    /// <returns>Lista de pedidos pendentes de ação administrativa.</returns>
    [HttpGet("Admin/ObterValidarVoluntariado")]
    [Authorize]
    public async Task<IActionResult> ValidarVoluntariado()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        int adminId = int.Parse(userIdClaim.Value);

        var admin = await _context.Utilizadores.FindAsync(adminId);
        if (admin == null || admin.TipoUtilizadorId != 2)
            return Forbid("Apenas administradores podem aceder a esta informação.");

        var pedidos = await _context.PedidosAjuda
            .Include(p => p.Voluntariados)
            .Include(p => p.Transacao)
            .Where(p =>
                p.Voluntariados.Any(v => v.Estado == EstadoVoluntariado.Pendente))
            .ToListAsync();

        var pedidosDTO = pedidos.Select(p => new PedidoPendenteDTO
        {
            PedidoId = p.PedidoId,
            Titulo = p.Titulo,
            Descricao = p.DescPedido,
            DataCriacao = p.HorarioAjuda,
            NumeroVoluntarios = p.Voluntariados.Count,
            Transacao = p.Transacao == null ? null : new TransacaoDTO
            {
                Id = p.Transacao.TransacaoId,
                DataTransacao = p.Transacao.Transacao.DataTransacao ?? DateTime.MinValue
            }
        }).ToList();

        return Ok(pedidosDTO);
    }
    
    
    /// <summary>
    /// Retorna todos os pedidos que requerem ações do administrador.
    /// </summary>
    /// <returns>Lista de pedidos pendentes de ação administrativa.</returns>
    [HttpGet("Admin/ObterValidarConclusaoPedido")]
    public async Task<IActionResult> ValidarConclusaoPedido()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        int adminId = int.Parse(userIdClaim.Value);

        var admin = await _context.Utilizadores.FindAsync(adminId);
        if (admin == null || admin.TipoUtilizadorId != 2)
            return Forbid("Apenas administradores podem aceder a esta informação.");

        var pedidos = await _context.PedidosAjuda
            .Include(p => p.Voluntariados)
            .Include(p => p.Transacao)
            .Where(p =>
                (p.Estado == EstadoPedido.Concluido && p.Transacao == null))
            .ToListAsync();

        var pedidosDTO = pedidos.Select(p => new PedidoPendenteDTO
        {
            PedidoId = p.PedidoId,
            Titulo = p.Titulo,
            Descricao = p.DescPedido,
            DataCriacao = p.HorarioAjuda,
            NumeroVoluntarios = p.Voluntariados.Count,
            Transacao = p.Transacao == null ? null : new TransacaoDTO
            {
                Id = p.Transacao.TransacaoId,
                DataTransacao = p.Transacao.Transacao.DataTransacao ?? DateTime.MinValue
            }
        }).ToList();
    
        return Ok(pedidosDTO);
    }

        [HttpGet("pedido/{pedidoId}/voluntarios")]
        public async Task<IActionResult> ObterVoluntariosPorPedido(int pedidoId)
        {
            var voluntarios = await _context.Voluntariados
                .Where(v => v.PedidoId == pedidoId && v.Estado != EstadoVoluntariado.Aceite) // Filtra para não incluir voluntários com estado 'Aceite'
                .Select(v => new
                {
                    Nome = v.Utilizador.NomeUtilizador,
                    UtilizadorId = v.UtilizadorId,
                    Contacto = v.Utilizador.Contactos
                        .Where(c => c.TipoContactoId == 1)
                        .Select(c => c.NumContacto)
                        .FirstOrDefault() ?? v.Utilizador.Contactos
                        .Where(c => c.TipoContactoId == 2)
                        .Select(c => c.NumContacto)
                        .FirstOrDefault()
                })
                .ToListAsync();

            if (voluntarios == null || !voluntarios.Any())
            {
                return NotFound(new { mensagem = "Nenhum voluntário encontrado para o pedido de ajuda." });
            }

            return Ok(voluntarios);
        }

        /// <summary>
        /// Obtém a foto do utilizador dono associado a um pedido de ajuda específico.
        /// </summary>
        /// <param name="pedidoAjudaId">O ID do pedido de ajuda.</param>
        /// <returns>A foto do utilizador dono.</returns>
        [HttpGet("{pedidoAjudaId}/foto-dono")]
        public async Task<ActionResult<string?>> GetFotoDono(int pedidoAjudaId)
        {
            try
            {
                var pedido = await _context.PedidosAjuda
                    .Where(p => p.PedidoId == pedidoAjudaId)
                    .Select(p => new
                    {
                        p.Utilizador.UtilizadorId,
                        p.Utilizador.NomeUtilizador,
                        p.Utilizador.FotoUtil
                    })
                    .FirstOrDefaultAsync();

                if (pedido == null)
                {
                    return NotFound();
                }

                if (pedido.FotoUtil == null)
                {
                    return NotFound();
                }

                return pedido.FotoUtil;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno do servidor.");
            }
        }



    }
}
