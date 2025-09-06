using EventosMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace EventosMVC.Services
{
    public class EventosService : IEventosService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;

        public EventosService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient();
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Pega token do claim do usuário logado
            var token = httpContextAccessor.HttpContext?.User?.FindFirst("JWT")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<IEnumerable<EventosViewModel>> GetAllEventosAsync()
        {
            var response = await _httpClient.GetAsync("http://localhost:8081/api/Eventos?api-version=2");
            if (response.IsSuccessStatusCode)
            {
                var eventos = await response.Content.ReadFromJsonAsync<IEnumerable<EventosViewModel>>(_options);
                return eventos ?? Enumerable.Empty<EventosViewModel>();
            }
            return Enumerable.Empty<EventosViewModel>();
        }

        public async Task<EventosViewModel> GetEventoByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"http://localhost:8081/api/Eventos/{id}?api-version=2");
            if (response.IsSuccessStatusCode)
            {
                var evento = await response.Content.ReadFromJsonAsync<EventosViewModel>(_options);
                return evento!;
            }
            return null!;
        }

        public async Task<(bool Sucesso, string Mensagem)> CreateEventoAsync(EventosViewModel eventoModel)
        {
            var dtoApi = new
            {
                titulo = eventoModel.Titulo,
                cidade = eventoModel.Cidade,
                entrada = eventoModel.Entrada,
                descricao = eventoModel.Descricao,
                data = eventoModel.Data,
                local = eventoModel.Local,
                vagas = eventoModel.Vagas,
                imagemUrl = eventoModel.ImagemUrl
            };

            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:8081/api/Eventos?api-version=2", dtoApi);

            if (response.IsSuccessStatusCode)
                return (true, "Evento criado com sucesso!");

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, $"Erro ao criar evento: {response.StatusCode}. {errorContent}");
        }

        public async Task<(bool Sucesso, string Mensagem)> UpdateEventoAsync(int id, EventosViewModel eventoModel)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"http://localhost:8081/api/Eventos/{id}?api-version=2", eventoModel);

            if (response.IsSuccessStatusCode)
                return (true, "Evento atualizado com sucesso!");

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, $"Erro ao atualizar evento: {response.StatusCode}. {errorContent}");
        }

       
        public async Task<(bool Sucesso, string Mensagem)> DeleteEventoAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"http://localhost:8081/api/Eventos/{id}?api-version=2");

            if (response.IsSuccessStatusCode)
                return (true, "Evento removido com sucesso!");

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, $"Erro ao apagar evento: {response.StatusCode}. {errorContent}");
        }

    }
}
