using EventosMVC.Models;

namespace EventosMVC.Services
{
    public interface IParticipantesService
    {
        Task<ParticipantesViewModel?> GetByIdAsync(int id);
        Task<ParticipantesViewModel?> CreateAsync(ParticipantesViewModel participante, string jwtToken);
        Task<ParticipantesViewModel?> UpdateAsync(int id, ParticipantesViewModel participante);

        Task<ParticipantesViewModel?> GetByEmailAsync(string email, string token);
    }
}
