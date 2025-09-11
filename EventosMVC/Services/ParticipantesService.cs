using EventosMVC.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EventosMVC.Services
{
    public class ParticipantesService : IParticipantesService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;

        public ParticipantesService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient();
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Definir BaseAddress da API
            if (_httpClient.BaseAddress == null)
                _httpClient.BaseAddress = new Uri("http://localhost:8083/");

            // Pegar JWT do cookie/claim do usuário logado
            var token = httpContextAccessor.HttpContext?.User?.FindFirst("JWT")?.Value;
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<ParticipantesViewModel?> GetByIdAsync(int id)
        {
            try
            {
                var participante = await _httpClient.GetFromJsonAsync<ParticipantesViewModel>(
                    $"api/participantes/{id}?api-version=2", _options);

                if (participante == null)
                    Console.WriteLine($"Participante {id} não encontrado.");

                return participante;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro HTTP ao buscar participante {id}: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado ao buscar participante {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<ParticipantesViewModel?> CreateAsync(ParticipantesViewModel participante, string jwtToken)
        {
            try
            {
                // Adiciona o token JWT no cabeçalho
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                var response = await _httpClient.PostAsJsonAsync(
                    $"api/participantes?api-version=2", participante);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Falha ao criar participante: {response.StatusCode} - {content}");
                    return null;
                }

                var created = await response.Content.ReadFromJsonAsync<ParticipantesViewModel>(_options);
                return created;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro HTTP ao criar participante: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado ao criar participante: {ex.Message}");
                return null;
            }
        }


        public async Task<ParticipantesViewModel?> UpdateAsync(int id, ParticipantesViewModel participante)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/participantes/{id}?api-version=2", participante);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Falha ao atualizar participante {id}: {response.StatusCode} - {content}");
                    return null;
                }

                var updated = await response.Content.ReadFromJsonAsync<ParticipantesViewModel>(_options);
                return updated;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro HTTP ao atualizar participante {id}: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado ao atualizar participante {id}: {ex.Message}");
                return null;
            }


        }
        public async Task<ParticipantesViewModel?> GetByEmailAsync(string email, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Supondo que a API tenha um endpoint para buscar participante pelo email
            var response = await _httpClient.GetAsync($"http://localhost:8083/api/Participantes/email/{email}?api-version=2");

            if (!response.IsSuccessStatusCode) return null;

            var participante = await response.Content.ReadFromJsonAsync<ParticipantesViewModel>(_options);
            return participante;
        }
    }
}
