using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommuniCare.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CommuniCare.DTOs;

namespace CommuniCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItensEmprestimoController : ControllerBase
    {
        private readonly CommuniCareContext _context;

        public ItensEmprestimoController(CommuniCareContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todos os itens de empréstimo.
        /// </summary>
        /// <returns>Uma lista de itens de empréstimo.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemEmprestimo>>> GetItemEmprestimos()
        {
            return await _context.ItensEmprestimo.ToListAsync();
        }

        /// <summary>
        /// Obtém todos os itens de empréstimo para um item específico.
        /// </summary>
        /// <param name="itemId">O ID do item.</param>
        /// <returns>Uma lista de itens de empréstimo do item especificado.</returns>
        [HttpGet("{itemId}")]
        public async Task<ActionResult<IEnumerable<ItemEmprestimo>>> GetItemEmprestimosPorItemId(int itemId)
        {
            var itensEmprestimo = await _context.ItensEmprestimo
                .Where(ie => ie.ItemId == itemId)
                .ToListAsync();

            if (itensEmprestimo == null || itensEmprestimo.Count == 0)
            {
                return NotFound();
            }

            return itensEmprestimo;
        }

        /// <summary>
        /// Obtém a foto do utilizador dono associado a um item de empréstimo específico.
        /// </summary>
        /// <param name="itemEmprestimoId">O ID do item de empréstimo.</param>
        /// <returns>A foto do utilizador dono.</returns>
        [HttpGet("{itemEmprestimoId}/foto-emprestador")]
        public async Task<ActionResult<string?>> GetFotoEmprestador(int itemEmprestimoId)
        {

            try
            {
                var relacaoEmprestador = await _context.ItemEmprestimoUtilizadores
                    .Where(rel => rel.ItemId == itemEmprestimoId && rel.TipoRelacao == "Dono")
                    .Select(rel => new
                    {
                        rel.Utilizador.UtilizadorId,
                        rel.Utilizador.NomeUtilizador,
                        rel.Utilizador.FotoUtil
                    })
                    .FirstOrDefaultAsync();

                if (relacaoEmprestador == null)
                {
                    return NotFound();
                }

                if (relacaoEmprestador.FotoUtil == null)
                {
                    return NotFound();
                }

                return relacaoEmprestador.FotoUtil;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno do servidor.");
            }
        }


        #region CONTROLLERS AUTOMÁTICOS
        /// <summary>
        /// Obtém um item de empréstimo específico pelo ID.
        /// </summary>
        /// <param name="id">O identificador do item de empréstimo a ser retornado.</param>
        /// <returns>O item de empréstimo correspondente ao ID ou NotFound se não encontrado.</returns>
        //[HttpGet("{id}")]
        //public async Task<ActionResult<ItemEmprestimo>> GetItemEmprestimo(int id)
        //{
        //    var itemEmprestimo = await _context.ItensEmprestimo.FindAsync(id);

        //    if (itemEmprestimo == null)
        //    {
        //        return NotFound();
        //    }

        //    return itemEmprestimo;
        //}


        ///// Atualiza os dados de um item de empréstimo existente.
        ///// </summary>
        ///// <param name="id">O identificador do item de empréstimo a ser atualizado.</param>
        ///// <param name="itemEmprestimo">Os novos dados do item de empréstimo.</param>
        ///// <returns>Resultado da operação: NoContent se bem-sucedido, BadRequest se IDs não corresponderem, NotFound se o item não existir.</returns>
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutItemEmprestimo(int id, ItemEmprestimo itemEmprestimo)
        //{
        //    if (id != itemEmprestimo.ItemId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(itemEmprestimo).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ItemEmprestimoExists(id))
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
        ///// Adiciona um novo item de empréstimo.
        ///// </summary>
        ///// <param name="itemEmprestimo">Os dados do item de empréstimo a ser adicionado.</param>
        ///// <returns>O item de empréstimo adicionado.</returns>
        //[HttpPost]
        //public async Task<ActionResult<ItemEmprestimo>> PostItemEmprestimo(ItemEmprestimo itemEmprestimo)
        //{
        //    _context.ItensEmprestimo.Add(itemEmprestimo);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetItemEmprestimo", new { id = itemEmprestimo.ItemId }, itemEmprestimo);
        //}

        ///// <summary>
        ///// Remove um item de empréstimo específico pelo ID.
        ///// </summary>
        ///// <param name="id">O identificador do item de empréstimo a ser removido.</param>
        ///// <returns>Resultado da operação: NoContent se bem-sucedido, NotFound se o item não existir.</returns>
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteItemEmprestimo(int id)
        //{
        //    var itemEmprestimo = await _context.ItensEmprestimo.FindAsync(id);
        //    if (itemEmprestimo == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.ItensEmprestimo.Remove(itemEmprestimo);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        #endregion

        /// <summary>
        /// Verifica se um item de empréstimo existe no banco de dados.
        /// </summary>
        /// <param name="id">O identificador do item a ser verificado.</param>
        /// <returns>True se o item existir, caso contrário False.</returns>
        private bool ItemEmprestimoExists(int id)
        {
            return _context.ItensEmprestimo.Any(e => e.ItemId == id);
        }

        /// <summary>
        /// Adiciona um novo item de empréstimo e notifica os administradores.
        /// </summary>
        /// <param name="itemData">Os dados do item de empréstimo a ser adicionado.</param>
        /// <returns>Mensagem de sucesso ou falha na adição.</returns>
        [HttpPost("AdicionarItem")]
        [Authorize]
        public async Task<IActionResult> AdicionarItemEmprestimo([FromBody] ItemEmprestimoDTO itemData)
        {
            if (itemData == null)
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

            var itemEmprestimo = new ItemEmprestimo
            {
                NomeItem = itemData.NomeItem,
                DescItem = itemData.DescItem,
                Disponivel = EstadoItemEmprestimo.Indisponivel,
                ComissaoCares = itemData.ComissaoCares,
                FotografiaItem = itemData.FotografiaItem
            };

            _context.ItensEmprestimo.Add(itemEmprestimo);
            await _context.SaveChangesAsync();

            var relacao = new ItemEmprestimoUtilizador
            {
                ItemId = itemEmprestimo.ItemId,
                UtilizadorId = utilizadorId,
                TipoRelacao = "Dono"
            };

            _context.ItemEmprestimoUtilizadores.Add(relacao);
            await _context.SaveChangesAsync();

            var admins = await _context.Utilizadores
                .Where(u => u.TipoUtilizadorId == 2)
                .ToListAsync();

            foreach (var admin in admins)
            {
                var notificacao = new Notificacao
                {
                    UtilizadorId = admin.UtilizadorId,
                    Mensagem = $"Um novo item '{itemEmprestimo.NomeItem}' foi adicionado e precisa ser validado.",
                    Lida = 0,
                    DataMensagem = DateTime.Now,
                    ItemId = itemEmprestimo.ItemId
                };

                _context.Notificacaos.Add(notificacao);
            }

            await _context.SaveChangesAsync();

            return Ok("Item de empréstimo adicionado com sucesso. Aguardando validação.");
        }


        /// <summary>
        /// Realiza o pedido de empréstimo de um item específico.
        /// </summary>
        /// <param name="itemId">O identificador do item a ser requisitado.</param>
        /// <returns>Mensagem de sucesso ou falha no pedido de empréstimo.</returns>
        [HttpPost("AdquirirItem/{itemId}")]
        [Authorize]
        public async Task<IActionResult> AdquirirItem(int itemId)
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

            var itemEmprestimo = await _context.ItensEmprestimo
                .Include(i => i.Utilizadores)
                .FirstOrDefaultAsync(i => i.ItemId == itemId);

            if (itemEmprestimo == null)
            {
                return NotFound("Item de empréstimo não encontrado.");
            }


            var jaRequisitou = await _context.ItemEmprestimoUtilizadores
                .AnyAsync(r => r.ItemId == itemId && r.UtilizadorId == utilizadorId && r.TipoRelacao == "Comprador");

            if (jaRequisitou)
            {
                return BadRequest("Você já requisitou este item anteriormente.");
            }


            var jaEDono = await _context.ItemEmprestimoUtilizadores
                .AnyAsync(r => r.ItemId == itemId && r.UtilizadorId == utilizadorId && r.TipoRelacao == "Dono");

            if (jaEDono)
            {
                return BadRequest("Não pode adquirir o item que você mesmo colocou.");
            }

            if (itemEmprestimo.Disponivel == EstadoItemEmprestimo.Indisponivel)
            {
                return BadRequest("Este item não está disponível para empréstimo.");
            }

            int comissao = itemEmprestimo.ComissaoCares ?? 0;
            if (utilizador.NumCares < comissao)
            {
                return BadRequest("Saldo de Cares insuficiente para adquirir este item.");
            }

            var emprestimo = new Emprestimo
            {
                DataIni = null,
                Items = new List<ItemEmprestimo> { itemEmprestimo }
            };

            _context.Emprestimos.Add(emprestimo);
            await _context.SaveChangesAsync();

            var relacao = new ItemEmprestimoUtilizador
            {
                ItemId = itemId,
                UtilizadorId = utilizadorId,
                TipoRelacao = "Comprador",
                EmprestimoId = emprestimo.EmprestimoId
            };

            _context.ItemEmprestimoUtilizadores.Add(relacao);
            _context.ItensEmprestimo.Update(itemEmprestimo);

            var admins = await _context.Utilizadores
                .Where(u => u.TipoUtilizadorId == 2)
                .ToListAsync();

            foreach (var admin in admins)
            {
                var notificacao = new Notificacao
                {
                    UtilizadorId = admin.UtilizadorId,
                    Mensagem = $"O item '{itemEmprestimo.NomeItem}' foi requisitado por '{utilizador.NomeUtilizador}'. Por favor, valide o empréstimo.",
                    Lida = 0,
                    DataMensagem = DateTime.Now,
                    ItemId = itemEmprestimo.ItemId
                };

                _context.Notificacaos.Add(notificacao);
            }

            await _context.SaveChangesAsync();

            return Ok("Pedido de empréstimo efetuado. Aguarde validação do administrador.");
        }



        #region Administrador

        /// <summary>
        /// Valida o item de empréstimo e torna-o disponível para empréstimo.
        /// Apenas administradores podem realizar essa ação.
        /// </summary>
        /// <param name="itemId">ID do item a ser validado.</param>
        /// <returns>Retorna um status indicando se a validação foi bem-sucedida ou se ocorreu um erro.</returns>
        [HttpPost("ValidarItem-(admin)/{itemId}")]
        [Authorize]
        public async Task<IActionResult> ValidarItemEmprestimo(int itemId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int adminId = int.Parse(userIdClaim.Value);

            var admin = await _context.Utilizadores.FindAsync(adminId);
            if (admin == null || admin.TipoUtilizadorId != 2)
                return Forbid("Apenas administradores podem validar itens.");

            var item = await _context.ItensEmprestimo
                .FirstOrDefaultAsync(i => i.ItemId == itemId);

            if (item == null)
                return NotFound("Item não encontrado.");

            if (item.Disponivel == EstadoItemEmprestimo.Disponivel)
                return BadRequest("Este item já está validado e disponível.");

            item.Disponivel = EstadoItemEmprestimo.Disponivel;


            var relacaoDono = await _context.ItemEmprestimoUtilizadores
                .FirstOrDefaultAsync(r => r.ItemId == itemId && r.TipoRelacao == "Dono");

            if (relacaoDono != null)
            {
                var notificacao = new Notificacao
                {
                    UtilizadorId = relacaoDono.UtilizadorId,
                    Mensagem = $"O teu item '{item.NomeItem}' foi validado por um administrador e está agora disponível para empréstimo!",
                    Lida = 0,
                    DataMensagem = DateTime.Now,
                    ItemId = item.ItemId
                };

                _context.Notificacaos.Add(notificacao);
            }

            _context.ItensEmprestimo.Update(item);
            await _context.SaveChangesAsync();

            return Ok("Item de empréstimo validado e tornado disponível com sucesso.");
        }

        /// <summary>
        /// Rejeita o item de empréstimo, removendo-o da plataforma e notificando o dono do item.
        /// Apenas administradores podem realizar essa ação.
        /// </summary>
        /// <param name="itemId">ID do item a ser rejeitado.</param>
        /// <returns>Retorna um status indicando se a rejeição foi bem-sucedida ou se ocorreu um erro.</returns>
        [HttpDelete("RejeitarItem-(admin)/{itemId}")]
        [Authorize]
        public async Task<IActionResult> RejeitarItemEmprestimo(int itemId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int adminId = int.Parse(userIdClaim.Value);

            var admin = await _context.Utilizadores.FindAsync(adminId);
            if (admin == null || admin.TipoUtilizadorId != 2)
                return Forbid("Apenas administradores podem rejeitar itens.");

            var item = await _context.ItensEmprestimo
                .FirstOrDefaultAsync(i => i.ItemId == itemId);

            if (item == null)
                return NotFound("Item não encontrado.");

            var relacaoDono = await _context.ItemEmprestimoUtilizadores
                .FirstOrDefaultAsync(r => r.ItemId == itemId && r.TipoRelacao == "Dono");

            var notificacoes = await _context.Notificacaos
                .Where(n => n.ItemId == itemId)
                .ToListAsync();

            foreach (var notificacao in notificacoes)
            {
                notificacao.ItemId = null;
            }

            if (relacaoDono != null)
            {
                var notificacao = new Notificacao
                {
                    UtilizadorId = relacaoDono.UtilizadorId,
                    Mensagem = $"O teu item '{item.NomeItem}' foi rejeitado por um administrador e não será adicionado à plataforma.",
                    Lida = 0,
                    DataMensagem = DateTime.Now,
                    ItemId = item.ItemId
                };

                _context.Notificacaos.Add(notificacao);
            }


            if (relacaoDono != null)
                _context.ItemEmprestimoUtilizadores.Remove(relacaoDono);

            _context.ItensEmprestimo.Remove(item);

            await _context.SaveChangesAsync();

            return Ok("Item rejeitado e removido com sucesso.");
        }



        #endregion

        /// <summary>
        /// Recupera os itens de empréstimo disponíveis para o utilizador.
        /// Retorna apenas itens que não são de propriedade do utilizador.
        /// </summary>
        /// <returns>Lista de itens disponíveis para empréstimo.</returns>
        [Authorize]
        [HttpGet("Disponiveis")]
        public async Task<ActionResult<IEnumerable<ItemEmprestimo>>> GetItensDisponiveis()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var meusItemIds = await _context.ItemEmprestimoUtilizadores
                .Where(rel => rel.UtilizadorId == utilizadorId && rel.TipoRelacao == "Dono")
                .Select(rel => rel.ItemId)
                .ToListAsync();

            var itensDisponiveis = await _context.ItensEmprestimo
                .Where(item => item.Disponivel == EstadoItemEmprestimo.Disponivel && !meusItemIds.Contains(item.ItemId))
                .ToListAsync();

            return Ok(itensDisponiveis);
        }

        /// <summary>
        /// Recupera os itens de empréstimo pertencentes ao utilizador.
        /// </summary>
        /// <returns>Lista de itens de empréstimo do utilizador.</returns>
        [Authorize]
        [HttpGet("MeusItens")]
        public async Task<ActionResult<IEnumerable<ItemEmprestimo>>> GetMeusItens()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var meusItemIds = await _context.ItemEmprestimoUtilizadores
                .Where(rel => rel.UtilizadorId == utilizadorId && rel.TipoRelacao == "Dono")
                .Select(rel => rel.ItemId)
                .ToListAsync();

            var meusItens = await _context.ItensEmprestimo
                .Where(item => meusItemIds.Contains(item.ItemId) && item.Disponivel != EstadoItemEmprestimo.IndisponivelPermanentemente)
                .ToListAsync();


            return Ok(meusItens);
        }

        /// <summary>
        /// Retorna os itens de empréstimo que requerem ações administrativas.
        /// </summary>
        /// <returns>Lista de itens com pendências administrativas.</returns>
        [HttpGet("Admin/ItensPendentes")]
        [Authorize]
        public async Task<IActionResult> ObterItensEmprestimoPendentesAdmin()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int adminId = int.Parse(userIdClaim.Value);

            var admin = await _context.Utilizadores.FindAsync(adminId);
            if (admin == null || admin.TipoUtilizadorId != 2)
                return Forbid("Apenas administradores podem rejeitar itens.");
                
            var itens = await _context.ItensEmprestimo
                .Include(i => i.Emprestimos)
                    .ThenInclude(e => e.Transacao)
                .Where(i =>
                    // Critério 1: Disponível = 0 mas não está marcado como permanentemente indisponível
                    i.Disponivel == 0 &&

                    // Critério 2: Algum empréstimo com DataInicio == null
                    i.Emprestimos.Any() == false
                )
                .ToListAsync();

            return Ok(itens);
        }

        /// <summary>
        /// Retorna os itens de empréstimo que requerem ações administrativas.
        /// </summary>
        /// <returns>Lista de itens com pendências administrativas.</returns>
        [HttpGet("Admin/ItensPendentes/Aquisicao")]
        [Authorize]
        public async Task<IActionResult> ObterItensEmprestimoAquisicaoAdmin()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int adminId = int.Parse(userIdClaim.Value);

            var admin = await _context.Utilizadores.FindAsync(adminId);
            if (admin == null || admin.TipoUtilizadorId != 2)
                return Forbid("Apenas administradores podem rejeitar itens.");

            var itens = await _context.ItensEmprestimo
                .Include(i => i.Emprestimos)
                    .ThenInclude(e => e.Transacao)
                .Where(i =>
                    i.Disponivel == EstadoItemEmprestimo.Disponivel &&
                    i.Emprestimos.Any(e => e.DataIni == null)
                )
                .ToListAsync();

            // Mapeando para DTOs
            var itensDTO = itens.Select(i => new ItemEmpPendenteDTO
            {
                ItemId = i.ItemId,
                NomeItem = i.NomeItem,
                DescItem = i.DescItem,
                ComissaoCares = i.ComissaoCares,
                FotografiaItem = i.FotografiaItem,
                Emprestimos = i.Emprestimos.Select(e => new EmprestimoDTO
                {
                    Id = e.EmprestimoId,
                    DataIni = e.DataIni,
                    Transacao = e.Transacao == null ? null : new TransacaoDTO
                    {
                        Id = e.Transacao.TransacaoId,
                        DataTransacao = e.Transacao.Transacao.DataTransacao
                    }
                }).ToList()
            }).ToList();

            return Ok(itensDTO);
        }

        /// <summary>
        /// Retorna os itens de empréstimo que requerem ações administrativas na devolução.
        /// </summary>
        /// <returns>Lista de itens com pendências de devolução.</returns>
        [HttpGet("Admin/ItensPendentes/Devolucao")]
        [Authorize]
        public async Task<IActionResult> ObterItensEmprestimoDevolucaoAdmin()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int adminId = int.Parse(userIdClaim.Value);

            var admin = await _context.Utilizadores.FindAsync(adminId);
            if (admin == null || admin.TipoUtilizadorId != 2)
                return Forbid("Apenas administradores podem ver esses itens.");

            var itens = await _context.ItensEmprestimo
                .Include(i => i.Emprestimos)
                    .ThenInclude(e => e.Transacao)
                .Where(i =>
                    i.Emprestimos.Any(e => e.Transacao == null) &&
                    i.Emprestimos.Any(e => e.DataDev != null)
                )
                .ToListAsync();

            // Mapeando para DTOs
            var itensDTO = itens.Select(i => new ItemEmpPendenteDTO
            {
                ItemId = i.ItemId,
                NomeItem = i.NomeItem,
                DescItem = i.DescItem,
                ComissaoCares = i.ComissaoCares,
                FotografiaItem = i.FotografiaItem,
                Emprestimos = i.Emprestimos
                    .Where(e => e.Transacao == null && e.DataDev != null)
                    .Select(e => new EmprestimoDTO
                    {
                        Id = e.EmprestimoId,
                        DataIni = e.DataIni,
                        DataDev = e.DataDev,
                        Transacao = null
                    })
                    .ToList()
            }).ToList();

            return Ok(itensDTO);
        }



        /// <summary>
        /// Remove permanentemente um item de empréstimo, tornando-o indisponível.
        /// Apenas o "Dono" do item pode realizar essa ação.
        /// </summary>
        /// <param name="itemId">ID do item a ser removido.</param>
        /// <returns>Retorna um status indicando se a remoção foi bem-sucedida ou se ocorreu um erro.</returns>
        [HttpDelete("IndisponibilizarPermanenteItem/{itemId}")]
        [Authorize]
        public async Task<IActionResult> EliminarItemEmprestimo(int itemId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId;
            if (!int.TryParse(userIdClaim.Value, out utilizadorId))
            {
                return Unauthorized("ID de utilizador inválido.");
            }

            var itemEmprestimo = await _context.ItensEmprestimo
                .FirstOrDefaultAsync(i => i.ItemId == itemId);

            if (itemEmprestimo == null)
            {
                return NotFound("Item de empréstimo não encontrado.");
            }

            var emprestimo = await _context.Emprestimos
                .Where(e => e.Items.Any(i => i.ItemId == itemId))
                .FirstOrDefaultAsync();


            var itemEmprestimoUtilizador = await _context.ItemEmprestimoUtilizadores
                .FirstOrDefaultAsync(ie => ie.ItemId == itemId && ie.UtilizadorId == utilizadorId);

            if (itemEmprestimoUtilizador == null)
            {
                return NotFound("Item de empréstimo ou relação de utilizador não encontrado.");
            }

            if (itemEmprestimoUtilizador.TipoRelacao != "Dono")
            {
                return Unauthorized("Você não tem permissão para alterar este item.");
            }

            itemEmprestimo.Disponivel = EstadoItemEmprestimo.IndisponivelPermanentemente;

            _context.ItensEmprestimo.Update(itemEmprestimo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Atualiza os campos de um item de empréstimo, exceto o número de pessoas.
        /// Apenas o "Dono" do item pode realizar essa atualização.
        /// </summary>
        /// <param name="itemId">ID do item a ser atualizado.</param>
        /// <param name="novaDescricao">Nova descrição do item.</param>
        /// <param name="novoNome">Novo nome do item.</param>
        /// <param name="novaComissaoCares">Nova comissão em Cares por hora.</param>
        /// <returns>Retorna um status indicando se a atualização foi bem-sucedida ou se ocorreu um erro.</returns>
        [HttpPut("AtualizarItem/{itemId}")]
        [Authorize]
        public async Task<IActionResult> AtualizarItem(int itemId, [FromBody] ItemEmprestimoDTO itemEmprestimoDTO)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId;
            if (!int.TryParse(userIdClaim.Value, out utilizadorId))
            {
                return Unauthorized("ID de utilizador inválido.");
            }

            var itemEmprestimo = await _context.ItensEmprestimo
                .FirstOrDefaultAsync(i => i.ItemId == itemId);

            if (itemEmprestimo == null)
            {
                return NotFound("Item de empréstimo não encontrado.");
            }

            var emprestimo = await _context.Emprestimos
                .Where(e => e.Items.Any(i => i.ItemId == itemId))
                .FirstOrDefaultAsync();

            var itemEmprestimoUtilizador = await _context.ItemEmprestimoUtilizadores
                .FirstOrDefaultAsync(ie => ie.ItemId == itemId && ie.UtilizadorId == utilizadorId);

            if (itemEmprestimoUtilizador == null)
            {
                return NotFound("Item de empréstimo ou relação de utilizador não encontrado.");
            }

            if (itemEmprestimoUtilizador.TipoRelacao != "Dono")
            {
                return Unauthorized("Você não tem permissão para atualizar este item.");
            }

            // Atualizando os campos, exceto o número de pessoas
            itemEmprestimo.NomeItem = itemEmprestimoDTO.NomeItem;
            itemEmprestimo.DescItem = itemEmprestimoDTO.DescItem;
            itemEmprestimo.ComissaoCares = itemEmprestimoDTO.ComissaoCares;
            itemEmprestimo.FotografiaItem = itemEmprestimoDTO.FotografiaItem;

            _context.ItensEmprestimo.Update(itemEmprestimo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Pesquisa um item de empréstimo pelo nome, de forma insensível a maiúsculas e minúsculas.
        /// O nome é transformado para minúsculas para garantir que a pesquisa não seja case sensitive.
        /// </summary>
        /// <param name="nome">Nome do item de empréstimo a ser pesquisado.</param>
        /// <returns>Retorna uma lista de itens que correspondem ao nome pesquisado.</returns>
        [HttpPost("PesquisarItemPorNome")]
        public async Task<IActionResult> PesquisarItemPorNome([FromBody] ItemEmprestimoDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.NomeItem))
            {
                return BadRequest("O nome do item não pode ser vazio.");
            }

            var nome = request.NomeItem.ToLower();

            var itensEncontrados = await _context.ItensEmprestimo
                .Where(i => i.NomeItem.ToLower().Contains(nome))
                .Select(i => new ItemEmprestimoDTO
                {
                    NomeItem = i.NomeItem,
                    DescItem = i.DescItem,
                    ComissaoCares = i.ComissaoCares,
                    FotografiaItem = i.FotografiaItem
                })
                .ToListAsync();

            if (!itensEncontrados.Any())
            {
                return NotFound("Nenhum item encontrado com esse nome.");
            }

            return Ok(itensEncontrados);
        }

        [HttpPut("atualizar-descricao/{itemId}")]
        [Authorize]
        public async Task<IActionResult> AtualizarDescricaoItem(int itemId, [FromBody] string novaDescricao)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId;
            if (!int.TryParse(userIdClaim.Value, out utilizadorId))
            {
                return Unauthorized("ID de utilizador inválido.");
            }

            var itemEmprestimo = await _context.ItensEmprestimo
                .FirstOrDefaultAsync(i => i.ItemId == itemId);

            if (itemEmprestimo == null)
            {
                return NotFound("Item de empréstimo não encontrado.");
            }

            var emprestimo = await _context.Emprestimos
                .Where(e => e.Items.Any(i => i.ItemId == itemId))
                .FirstOrDefaultAsync();

            if (emprestimo == null)
            {
                return NotFound("Empréstimo relacionado não encontrado.");
            }

            var itemEmprestimoUtilizador = await _context.ItemEmprestimoUtilizadores
                .FirstOrDefaultAsync(ie => ie.ItemId == itemId && ie.UtilizadorId == utilizadorId);

            if (itemEmprestimoUtilizador == null)
            {
                return NotFound("Item de empréstimo ou relação de utilizador não encontrado.");
            }

            if (itemEmprestimoUtilizador.TipoRelacao != "Dono")
            {
                return Unauthorized("Você não tem permissão para atualizar este item.");
            }

            itemEmprestimo.DescItem = novaDescricao;

            _context.ItensEmprestimo.Update(itemEmprestimo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Recupera os itens de empréstimo pertencentes ao utilizador que estão em uso.
        /// </summary>
        /// <returns>Lista de itens de empréstimo do utilizador que estão em uso.</returns>
        [Authorize]
        [HttpGet("MeusItensEmUso")]
        public async Task<ActionResult<IEnumerable<ItemEmprestimoDTO>>> GetMeusItensEmUso()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized("Utilizador não autenticado.");
            }

            int utilizadorId = int.Parse(userIdClaim.Value);

            var meusItemIds = await _context.ItemEmprestimoUtilizadores
                .Where(rel => rel.UtilizadorId == utilizadorId && rel.TipoRelacao == "Dono")
                .Select(rel => rel.ItemId)
                .ToListAsync();

            var itensEmUso = await _context.ItensEmprestimo
                .Include(i => i.Emprestimos)
                .Where(i =>
                    meusItemIds.Contains(i.ItemId) &&
                    i.Disponivel == 0 &&
                    i.Emprestimos.Any(e => e.DataIni != null && e.Transacao == null && e.DataDev == null)
                )
                .Select(i => new ItemEmprestimoDTO
                {
                    NomeItem = i.NomeItem,
                    DescItem = i.DescItem,
                    ComissaoCares = i.ComissaoCares,
                    FotografiaItem = i.FotografiaItem
                })
                .ToListAsync();

            return Ok(itensEmUso);
        }

        [Authorize]
        [HttpGet("ItensUtilizador/{id}")]
        public async Task<ActionResult<IEnumerable<ItemEmprestimo>>> GetItensDeOutroUtilizador(int id)
        {

            var existeUtilizador = await _context.Utilizadores
                .AsNoTracking()
                .AnyAsync(u => u.UtilizadorId == id);

            if (!existeUtilizador)
                return NotFound($"Utilizador com ID {id} não encontrado.");

            var itemIds = await _context.ItemEmprestimoUtilizadores
                .Where(rel => rel.UtilizadorId == id && rel.TipoRelacao == "Dono")
                .Select(rel => rel.ItemId)
                .ToListAsync();

            var itens = await _context.ItensEmprestimo
                .Where(item => itemIds.Contains(item.ItemId) &&
                               item.Disponivel != EstadoItemEmprestimo.IndisponivelPermanentemente)
                .ToListAsync();

            return Ok(itens);
        }

        [HttpGet("{itemEmprestimoId}/dados-emprestador")]
        public async Task<ActionResult<object>> GetDadosEmprestador(int itemEmprestimoId)
        {
            try
            {
                var relacaoEmprestador = await _context.ItemEmprestimoUtilizadores
                    .Where(rel => rel.ItemId == itemEmprestimoId && rel.TipoRelacao == "Dono")
                    .Select(rel => new
                    {
                        IdEmprestador = rel.Utilizador.UtilizadorId,
                        rel.Utilizador.FotoUtil
                    })
                    .FirstOrDefaultAsync();

                if (relacaoEmprestador == null)
                {
                    return NotFound();
                }

                if (relacaoEmprestador.FotoUtil == null)
                {
                    return NotFound();
                }
                return Ok(relacaoEmprestador);
            }
            catch (Exception)
            {
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}
