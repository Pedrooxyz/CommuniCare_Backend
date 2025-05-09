namespace CommuniCare.DTOs
{
    public class EmprestimoDTO
    {
        public int Id { get; set; }
        public DateTime? DataIni { get; set; }
        public DateTime? DataDev { get; set; }
        public TransacaoDTO? Transacao { get; set; }
    }
}
