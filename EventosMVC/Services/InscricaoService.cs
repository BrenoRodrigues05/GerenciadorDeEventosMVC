using EventosMVC.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace EventosMVC.Services
{
    public class InscricaoService : IInscricaoService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;

        public InscricaoService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // GET: Todas as inscrições
        public async Task<IEnumerable<InscricaoViewModel>> GetAllInscricoesAsync()
        {
            var response = await _httpClient.GetAsync("http://localhost:8081/api/Inscricao?api-version=2");
            if (response.IsSuccessStatusCode)
            {
                var inscricoes = await response.Content.ReadFromJsonAsync<IEnumerable<InscricaoViewModel>>(_options);
                return inscricoes ?? Enumerable.Empty<InscricaoViewModel>();
            }
            return Enumerable.Empty<InscricaoViewModel>();
        }

        // POST: Criar nova inscrição
        public async Task<InscricaoViewModel> CreateInscricaoAsync(InscricaoViewModel inscricaoModel)
        {
            var response = await _httpClient.PostAsJsonAsync("http://localhost:8081/api/Inscricao?api-version=2", inscricaoModel);
            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<InscricaoViewModel>(_options);
                return created!;
            }
            return null!;
        }
        public async Task<IEnumerable<EventosViewModel>> GetEventosDisponiveisAsync()
        {
            // Pega todos os eventos
            var response = await _httpClient.GetAsync("http://localhost:8081/api/Eventos?api-version=2");
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<EventosViewModel>();

            var eventos = await response.Content.ReadFromJsonAsync<IEnumerable<EventosViewModel>>(_options);
            if (eventos == null) return Enumerable.Empty<EventosViewModel>();

            // Filtra apenas eventos com vagas
            var eventosDisponiveis = eventos.Where(e => e.Vagas > 0);

            return eventosDisponiveis;
        }

    }
}
