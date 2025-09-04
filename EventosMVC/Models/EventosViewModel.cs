using System.ComponentModel.DataAnnotations;

namespace EventosMVC.Models
{
    public class EventosViewModel
    {
        [Display(Name = "ID do Evento")]
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(100, ErrorMessage = "O título pode ter no máximo 100 caracteres.")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A cidade é obrigatória.")]
        [Display(Name = "Cidade")]
        public string Cidade { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Entrada")]
        public string Entrada { get; set; } = "Gratuita"; // Entrada padrão como "Gratuita"

        [Required]
        [StringLength(500, ErrorMessage = "A descrição pode ter no máximo 500 caracteres.")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data do evento é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data do Evento")]
        public DateTime Data { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "O local do evento é obrigatório.")]
        [StringLength(200, ErrorMessage = "O local pode ter no máximo 200 caracteres.")]
        [Display(Name = "Local")]
        public string Local { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "O número de vagas deve ser maior que zero.")]
        [Display(Name = "Vagas")]
        public int Vagas { get; set; } = 1;

        [Display(Name = "Imagem do Local do Evento")]

        public string ImagemUrl { get; set; } = string.Empty;

    }
}
