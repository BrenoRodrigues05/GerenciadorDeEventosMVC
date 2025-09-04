using System.ComponentModel.DataAnnotations;

namespace EventosMVC.Models
{
    public class ParticipantesViewModel
    {
        [Display(Name = "ID do Participante")]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail não é válido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "O telefone não é válido.")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; } = string.Empty;

    }
}
