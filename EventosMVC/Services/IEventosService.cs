using EventosMVC.Models;

namespace EventosMVC.Services
{
    public interface IEventosService
    {
        Task<IEnumerable<EventosViewModel>> GetAllEventosAsync();

        Task<EventosViewModel> GetEventoByIdAsync(int id);

        // Retorna sucesso e mensagem para exibir no MVC
        Task<(bool Sucesso, string Mensagem)> CreateEventoAsync(EventosViewModel eventoModel);

        Task<(bool Sucesso, string Mensagem)> UpdateEventoAsync(int id, EventosViewModel eventoModel);

        Task<(bool Sucesso, string Mensagem)> DeleteEventoAsync(int id);
    }
}
