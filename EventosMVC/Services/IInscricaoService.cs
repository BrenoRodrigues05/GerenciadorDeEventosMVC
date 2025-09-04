using EventosMVC.Models;

namespace EventosMVC.Services
{
    public interface IInscricaoService
    {
        Task<IEnumerable<InscricaoViewModel>> GetAllInscricoesAsync();
        Task<InscricaoViewModel> CreateInscricaoAsync(InscricaoViewModel inscricaoModel);
    }
}
