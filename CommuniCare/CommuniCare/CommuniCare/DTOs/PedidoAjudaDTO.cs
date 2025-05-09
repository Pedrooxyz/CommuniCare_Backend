namespace CommuniCare.DTOs
{
    public class PedidoAjudaDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public string DescPedido { get; set; }
        public DateTime HorarioAjuda { get; set; }
        public int NHoras { get; set; }
        public int NPessoas { get; set; }
        public string? FotografiaPA { get; set; }
    }
}
