namespace EventosMVC.DTOs
{
    public class InscricaoDTO
    {
        public int Id { get; set; }
        public int EventoId { get; set; }
        public string? NomeEvento { get; set; }
        public int ParticipanteId { get; set; }
        public string? NomeParticipante { get; set; }
        public DateTime? DataInscricao { get; set; }
    }
}
