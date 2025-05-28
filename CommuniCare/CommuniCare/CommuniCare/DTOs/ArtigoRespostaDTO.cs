namespace CommuniCare.DTOs
{
    public class ArtigoRespostaDto
    {
        public int ArtigoId { get; set; }
        public string? NomeArtigo { get; set; }
        public string? DescArtigo { get; set; }
        public int? CustoCares { get; set; }
        public int? QuantidadeDisponivel { get; set; }
        public int LojaId { get; set; }

        public string? FotografiaArt { get; set; }
    }

}
