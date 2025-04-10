using System.ComponentModel.DataAnnotations;

namespace CommuniCare.DTOs
{
    public class NovaSenhaDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NovaSenha { get; set; }
    }
}
