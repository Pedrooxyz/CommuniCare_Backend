namespace CommuniCare.DTOs
{
    public class ArtigoDto
    {
        public string? NomeArtigo { get; set; }
        public string? DescArtigo { get; set; }
        public int? CustoCares { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public int LojaId { get; set; }
    }

}