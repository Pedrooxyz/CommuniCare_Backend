using System.ComponentModel.DataAnnotations;

namespace CommuniCare.DTOs
{
    public class RecuperarSenhaDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
