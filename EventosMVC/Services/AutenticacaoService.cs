using EventosMVC.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace EventosMVC.Services
{
    public class AutenticacaoService : IAutenticação
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _options;

        public AutenticacaoService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<TokenViewModel> Autenticar(UsuarioViewModel usuario)
        {
            var client = _httpClientFactory.CreateClient("AuthApi");

            var jsonUsuario = JsonSerializer.Serialize(usuario, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonUsuario, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("AuthControllerV/login", content);

            var jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Resposta da API:");
            Console.WriteLine(jsonString);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException($"Erro na autenticação: {response.StatusCode}");
            }

            var tokenViewModel = JsonSerializer.Deserialize<TokenViewModel>(jsonString, _options)!;

            return tokenViewModel;
        }

        public async Task<ResultadoViewModel> Registrar(RegistroViewModel usuario)
        {
            var client = _httpClientFactory.CreateClient("AuthApi");

            var jsonUsuario = JsonSerializer.Serialize(usuario, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonUsuario, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("AuthControllerV/register", content);

            var jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Resposta do registro:");
            Console.WriteLine(jsonString);

            if (!response.IsSuccessStatusCode)
            {
                return new ResultadoViewModel
                {
                    Sucesso = false,
                    Mensagem = $"Erro {response.StatusCode}: {response.ReasonPhrase}"
                };
            }

            return new ResultadoViewModel
            {
                Sucesso = true,
                Mensagem = "Usuário registrado com sucesso!"
            };
        }

        public static void PutToken(string token, HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}
