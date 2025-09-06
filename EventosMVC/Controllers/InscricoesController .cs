using EventosMVC.Models;
using EventosMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace EventosMVC.Controllers
{
    [Authorize] // Qualquer usuário logado
    public class InscricoesController : Controller
    {
        private readonly IInscricaoService _inscricaoService;

        public InscricoesController(IInscricaoService inscricaoService)
        {
            _inscricaoService = inscricaoService;
        }

        // GET: Inscricoes - Lista apenas as inscrições do usuário logado
        public async Task<IActionResult> Index()
        {
            var participanteId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var todas = await _inscricaoService.GetAllInscricoesAsync() ?? new List<InscricaoViewModel>();
            var minhasInscricoes = todas.Where(i => i.ParticipanteId == participanteId);

            return View(minhasInscricoes);
        }

        // GET: Inscricoes/Create - Formulário para nova inscrição
        public async Task<IActionResult> Create(int? eventoId)
        {
            // Eventos disponíveis (com vagas)
            var eventos = await _inscricaoService.GetEventosDisponiveisAsync();
            ViewBag.EventosDisponiveis = new SelectList(
                eventos.Select(e => new { e.Id, Nome = $"{e.Titulo} - {e.Data:dd/MM/yyyy} - {e.Cidade}" }),
                "Id",
                "Nome",
                 eventoId // seleciona automaticamente se veio por rota
            );

            var participanteId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var model = new InscricaoViewModel
            {
                ParticipanteId = participanteId,
                EventoId = eventoId ?? 0
            };

            return View(model);
        }

        // POST: Inscricoes/Create - Criação da inscrição
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InscricaoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recarregar dropdown
                var eventos = await _inscricaoService.GetEventosDisponiveisAsync();
                ViewBag.EventosDisponiveis = new SelectList(
                    eventos.Select(e => new { e.Id, Nome = $"{e.Titulo} - {e.Data:dd/MM/yyyy} - {e.Cidade}" }),
                    "Id",
                    "Nome"
                );

                return View(model);
            }

            try
            {
                // Garantir que o participante logado é o correto
                model.ParticipanteId = int.Parse(User.FindFirst("sub")?.Value ?? "0");

                var created = await _inscricaoService.CreateInscricaoAsync(model);

                TempData["MensagemSucesso"] = $"Inscrição criada com sucesso para {created.ParticipanteNome}!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao criar inscrição: {ex.Message}";

                // Recarregar dropdown
                var eventos = await _inscricaoService.GetEventosDisponiveisAsync();
                ViewBag.EventosDisponiveis = new SelectList(
                    eventos.Select(e => new { e.Id, Nome = $"{e.Titulo} - {e.Data:dd/MM/yyyy} - {e.Cidade}" }),
                    "Id",
                    "Nome"
                );

                return View(model);
            }
        }

        // GET: Inscricoes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var todas = await _inscricaoService.GetAllInscricoesAsync();
            var inscricao = todas.FirstOrDefault(i => i.Id == id);

            if (inscricao == null)
            {
                TempData["MensagemErro"] = "Inscrição não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Garantir que o usuário só veja suas inscrições
            var participanteId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            if (inscricao.ParticipanteId != participanteId)
            {
                TempData["MensagemErro"] = "Acesso negado a esta inscrição.";
                return RedirectToAction(nameof(Index));
            }

            return View(inscricao);
        }
    }
}
