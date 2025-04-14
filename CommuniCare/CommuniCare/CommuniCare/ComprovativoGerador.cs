using CommuniCare.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace CommuniCare
{
    public static class ComprovativoGenerator
    {
        public static byte[] GerarComprovativoPDF(Venda venda, Utilizador user, List<Artigo> artigos)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Content()
                        .Column(col =>
                        {
                            col.Item().Text($"Comprovativo de Compra - #{venda.TransacaoId}").Bold().FontSize(20);
                            col.Item().Text($"Utilizador: {user.NomeUtilizador}");
                            col.Item().Text($"Data: {DateTime.Now}");
                            col.Item().Text("Artigos:");
                            foreach (var artigo in artigos)
                                col.Item().Text($"- {artigo.NomeArtigo} ({artigo.CustoCares} cares)");

                            col.Item().Text($"Total: {artigos.Sum(a => a.CustoCares)} cares").Bold();
                        });
                });
            }).GeneratePdf();
        }
    }
}

