namespace CommuniCare.DTOs
{
    public class ItemEmpPendenteDTO
    {
        public string? NomeItem { get; set; }
        public string? DescItem { get; set; }
        public int? ComissaoCares { get; set; }
        public string? FotografiaItem { get; set; }
        public List<EmprestimoDTO> Emprestimos { get; set; }
    }
}
