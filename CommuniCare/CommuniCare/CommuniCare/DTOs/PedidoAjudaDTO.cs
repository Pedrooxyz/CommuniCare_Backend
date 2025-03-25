namespace CommuniCare.DTOs
{
    public class PedidoAjudaDTO
    {
        public int UtilizadorId { get; set; }
        public string DescPedido { get; set; }
        public DateTime HorarioAjuda { get; set; }
        public int NHoras { get; set; }
        public int NPessoas { get; set; }
    }
}
