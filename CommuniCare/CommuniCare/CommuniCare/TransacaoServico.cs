using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommuniCare.Models;
using Microsoft.EntityFrameworkCore;


namespace CommuniCare
{
    public class TransacaoServico
    {
        private readonly CommuniCareContext _context;

        public TransacaoServico(CommuniCareContext context)
        {
            _context = context;
        }

        public async Task<(Venda venda, Transacao transacao, List<Artigo> artigos, DateTime dataCompra)> ProcessarCompraAsync(int userId, List<int> artigosIds)
        {
            var user = await _context.Utilizadores.FindAsync(userId);
            if (user == null)
                throw new Exception("Utilizador não encontrado.");

            var artigos = await _context.Artigos
                .Where(a => artigosIds.Contains(a.ArtigoId))
                .ToListAsync();

            if (artigos.Count != artigosIds.Count)
                throw new Exception("Alguns artigos não foram encontrados.");

            int totalCares = artigos.Sum(a => a.CustoCares ?? 0);
            if (user.NumCares < totalCares)
                throw new Exception("Pontos insuficientes.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var dataCompra = DateTime.Now;

                var transacao = new Transacao
                {
                    DataTransacao = dataCompra,
                    Quantidade = artigos.Count
                };
                _context.Transacaos.Add(transacao);
                await _context.SaveChangesAsync();

                var venda = new Venda
                {
                    UtilizadorId = userId,
                    TransacaoId = transacao.TransacaoId,
                    NArtigos = artigos.Count
                };
                _context.Venda.Add(venda);
                await _context.SaveChangesAsync();

                foreach (var artigo in artigos)
                {
                    artigo.TransacaoId = transacao.TransacaoId;
                }

                user.NumCares -= totalCares;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (venda, transacao, artigos, dataCompra);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception("Erro ao processar a transação.");
            }
        }
    }
}