namespace CommuniCare.DTOs
{
    public class PedidoAjudaEditDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public string DescPedido { get; set; }
        public int NHoras { get; set; }
        public int NPessoas { get; set; }
    }
}