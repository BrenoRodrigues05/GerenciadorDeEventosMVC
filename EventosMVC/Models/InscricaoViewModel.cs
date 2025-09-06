using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventosMVC.Models
{
    public class InscricaoViewModel
    {
        [Display(Name = "ID da Inscrição")]
        public int Id { get; set; }

        [Required(ErrorMessage = "O Evento é obrigatório.")]
        [Display(Name = "Evento")]
        public int EventoId { get; set; }

        [Required(ErrorMessage = "O Nome do Evento é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome do evento deve ter no máximo 100 caracteres.")]
        [Display(Name = "Nome do Evento")]
        public string EventoNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Participante é obrigatório.")]
        [Display(Name = "Participante")]
        public int ParticipanteId { get; set; }

        [Required(ErrorMessage = "O Nome do Participante é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome do participante deve ter no máximo 100 caracteres.")]
        [Display(Name = "Nome do Participante")]
        public string ParticipanteNome { get; set; } = string.Empty;

        [Display(Name = "Data de Inscrição")]
        [DataType(DataType.Date)]
        public DateTime DataInscricao { get; set; } = DateTime.Now;

        [Display(Name = "Nome Do evento")]
        public string NomeEvento => $"{EventoNome} (ID: {EventoId})";

        [Display(Name = "Data do Evento")]
        public DateTime DataEvento { get; set; }
        [Display(Name = "Local do Evento")]
        public string LocalEvento { get; set; } = string.Empty; 
    }    
}
