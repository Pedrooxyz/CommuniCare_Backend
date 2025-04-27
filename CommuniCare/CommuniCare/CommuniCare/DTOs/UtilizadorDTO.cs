using System.ComponentModel.DataAnnotations;

namespace CommuniCare.DTOs
{
    public class UtilizadorDTO
    {
        [Required]
        public string NomeUtilizador { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }


    public class UtilizadorInfoDto
    {
        public int UtilizadorId { get; set; }

        public string? NomeUtilizador { get; set; }

        public string? FotoUtil { get; set; }

        public int? NumCares { get; set; }

        public int MoradaId { get; set; }

        public int TipoUtilizadorId { get; set; }
    }
}
