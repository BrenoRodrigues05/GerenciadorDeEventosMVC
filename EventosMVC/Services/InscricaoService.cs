using EventosMVC.DTOs;
using EventosMVC.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace EventosMVC.Services
{
    public class InscricaoService : IInscricaoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _options;

        public InscricaoService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // 🔹 Método auxiliar para criar HttpClient com token atualizado
        private HttpClient GetHttpClient()
        {
            var client = _httpClientFactory.CreateClient();

            // Pega o token do claim "JWT" do usuário logado
            var token = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == "JWT")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        // 🔹 Buscar todas as inscrições
        public async Task<IEnumerable<InscricaoViewModel>> GetAllInscricoesAsync()
        {
            var client = GetHttpClient();
            var response = await client.GetAsync("http://localhost:8083/api/Inscricao?api-version=2");
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<InscricaoViewModel>();

            var inscricoes = await response.Content.ReadFromJsonAsync<IEnumerable<InscricaoDTO>>(_options);
            if (inscricoes == null) return Enumerable.Empty<InscricaoViewModel>();

            var lista = new List<InscricaoViewModel>();
            foreach (var i in inscricoes)
            {
                // Buscar dados do evento
                var eventoResp = await client.GetAsync($"http://localhost:8083/api/Eventos/{i.EventoId}?api-version=2");
                var evento = eventoResp.IsSuccessStatusCode
                    ? await eventoResp.Content.ReadFromJsonAsync<EventosViewModel>(_options)
                    : null;

                // Buscar dados do participante
                var participanteResp = await client.GetAsync($"http://localhost:8083/api/Participantes/{i.ParticipanteId}?api-version=2");
                var participante = participanteResp.IsSuccessStatusCode
                    ? await participanteResp.Content.ReadFromJsonAsync<ParticipantesViewModel>(_options)
                    : null;

                lista.Add(new InscricaoViewModel
                {
                    Id = i.Id,
                    EventoId = i.EventoId,
                    EventoNome = evento?.Titulo ?? "Evento desconhecido",
                    DataEvento = evento?.Data ?? DateTime.MinValue,
                    LocalEvento = evento?.Cidade ?? "",
                    ParticipanteId = i.ParticipanteId,
                    ParticipanteNome = participante?.Nome ?? "Participante desconhecido",
                    DataInscricao = i.DataInscricao
                });
            }

            return lista;
        }

        // 🔹 Criar nova inscrição
        public async Task<InscricaoViewModel?> CreateInscricaoAsync(InscricaoViewModel model)
        {
            model.DataInscricao ??= DateTime.Now;
            var client = GetHttpClient();

            var payload = new InscricaoDTO
            {
                EventoId = model.EventoId,
                ParticipanteId = model.ParticipanteId,
                DataInscricao = model.DataInscricao
            };

            var response = await client.PostAsJsonAsync("http://localhost:8083/api/Inscricao?api-version=2", payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro ao criar inscrição: {response.StatusCode} - {error}");
                return null;
            }

            var created = await response.Content.ReadFromJsonAsync<InscricaoDTO>(_options);
            if (created == null) return null;

            // Buscar dados do evento
            var eventoResp = await client.GetAsync($"http://localhost:8083/api/Eventos/{created.EventoId}?api-version=2");
            var evento = eventoResp.IsSuccessStatusCode
                ? await eventoResp.Content.ReadFromJsonAsync<EventosViewModel>(_options)
                : null;

            // Buscar dados do participante
            var participanteResp = await client.GetAsync($"http://localhost:8083/api/Participantes/{created.ParticipanteId}?api-version=2");
            var participante = participanteResp.IsSuccessStatusCode
                ? await participanteResp.Content.ReadFromJsonAsync<ParticipantesViewModel>(_options)
                : null;

            return new InscricaoViewModel
            {
                Id = created.Id,
                EventoId = created.EventoId,
                EventoNome = evento?.Titulo ?? "Evento desconhecido",
                DataEvento = evento?.Data ?? DateTime.MinValue,
                LocalEvento = evento?.Cidade ?? "",
                ParticipanteId = created.ParticipanteId,
                ParticipanteNome = participante?.Nome ?? "Participante desconhecido",
                DataInscricao = created.DataInscricao
            };
        }

        // 🔹 Buscar eventos disponíveis
        public async Task<IEnumerable<EventosViewModel>> GetEventosDisponiveisAsync()
        {
            var client = GetHttpClient();
            var response = await client.GetAsync("http://localhost:8083/api/Eventos?api-version=2");
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<EventosViewModel>();

            var eventos = await response.Content.ReadFromJsonAsync<IEnumerable<EventosViewModel>>(_options);
            return eventos?.Where(e => e.Vagas > 0) ?? Enumerable.Empty<EventosViewModel>();
        }
    }
}
