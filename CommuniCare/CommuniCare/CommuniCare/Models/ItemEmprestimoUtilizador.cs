namespace CommuniCare.Models
{
    public class ItemEmprestimoUtilizador
    {
        public int ItemEmpId { get; set; } 

        public int ItemId { get; set; }
        public int? UtilizadorId { get; set; }

        public string TipoRelacao { get; set; }

        public int? EmprestimoId { get; set; }
        public virtual Emprestimo Emprestimo { get; set; }

        public virtual ItemEmprestimo ItemEmprestimo { get; set; }
        public virtual Utilizador Utilizador { get; set; }
    }


}
