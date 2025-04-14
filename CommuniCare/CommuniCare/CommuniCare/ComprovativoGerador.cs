using CommuniCare.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace CommuniCare
{
    public static class ComprovativoGenerator
    {
        // Método para gerar o código do voucher
        public static string GerarCodigoVoucher()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var code = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"COMM-2025-{code}";
        }


        public static byte[] GerarComprovativoUnicoPDF(Venda venda, Utilizador user, List<Artigo> artigos)
        {
            var comprovativo = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Helvetica"));

                    page.Content().Column(col =>
                    {
                        // Cabeçalho
                        col.Item().Text("🧾 COMPROVATIVO DE COMPRA")
                            .FontSize(22)
                            .Bold()
                            .FontColor(Colors.Blue.Medium);

                        col.Item().PaddingTop(5).Text($"👤 Utilizador: {user.NomeUtilizador}")
                            .FontSize(14);

                        col.Item().Text($"📅 Data da Compra: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(12)
                            .FontColor(Colors.Grey.Darken2);

                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                        // Lista de artigos
                        foreach (var artigo in artigos)
                        {
                            var voucherCode = GerarCodigoVoucher();

                            col.Item().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Column(artigoCol =>
                            {
                                artigoCol.Item().Text($"🛍️ Artigo: {artigo.NomeArtigo}")
                                    .FontSize(14)
                                    .Bold();

                                artigoCol.Item().Text($"💰 Custo: {artigo.CustoCares} cares")
                                    .FontSize(13);

                                artigoCol.Item().PaddingTop(5).Text("🎟️ Código do Voucher:")
                                    .FontSize(13)
                                    .Bold();

                                artigoCol.Item().Container()
                                    .Background(Colors.Grey.Lighten4)
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Medium)
                                    .Padding(8)
                                    .AlignCenter()
                                    .Text(voucherCode)
                                        .FontSize(16)
                                        .Bold()
                                        .FontColor(Colors.Green.Darken2);
                            });
                        }

                        col.Item().PaddingTop(20).AlignCenter().Text("Obrigado por usar a CommuniCare 💙")
                            .FontSize(12)
                            .FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            return comprovativo.GeneratePdf();
        }


    }
}

