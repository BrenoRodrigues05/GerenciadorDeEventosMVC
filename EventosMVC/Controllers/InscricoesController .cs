using EventosMVC.Models;
using EventosMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventosMVC.Controllers
{
    [Authorize] // Apenas usuários logados
    public class InscricoesController : Controller
    {
        private readonly IInscricaoService _inscricaoService;
        private readonly IParticipantesService _participantesService;

        public InscricoesController(
            IInscricaoService inscricaoService,
            IParticipantesService participantesService)
        {
            _inscricaoService = inscricaoService;
            _participantesService = participantesService;
        }

        // 🔹 Recupera participante logado via claim "sub"
        private async Task<ParticipantesViewModel?> ObterParticipanteLogadoAsync()
        {
            var subClaim = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(subClaim) || !int.TryParse(subClaim, out int participanteId))
                return null;

            return await _participantesService.GetByIdAsync(participanteId);
        }

        // 🔹 Preenche dropdown de eventos disponíveis
        private async Task PreencherEventosDisponiveisAsync(int? eventoId = null)
        {
            var eventos = await _inscricaoService.GetEventosDisponiveisAsync();
            ViewBag.EventosDisponiveis = new SelectList(
                eventos.Select(e => new { e.Id, Nome = $"{e.Titulo} - {e.Data:dd/MM/yyyy} - {e.Cidade}" }),
                "Id",
                "Nome",
                eventoId
            );
        }

        // GET: Inscricoes
        public async Task<IActionResult> Index()
        {
            var participante = await ObterParticipanteLogadoAsync();
            if (participante == null)
                return RedirectToAction("Index", "Eventos");

            var todas = await _inscricaoService.GetAllInscricoesAsync();
            var minhasInscricoes = todas.Where(i => i.ParticipanteId == participante.Id);

            return View(minhasInscricoes);
        }

        // GET: Inscricoes/Create
        public async Task<IActionResult> Create(int? eventoId)
        {
            var participante = await ObterParticipanteLogadoAsync();
            if (participante == null)
                return RedirectToAction("Index", "Eventos");

            var model = new InscricaoViewModel
            {
                ParticipanteId = participante.Id,
                EventoId = eventoId ?? 0
            };

            await PreencherEventosDisponiveisAsync(model.EventoId);
            return View(model);
        }

        // POST: Inscricoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InscricaoViewModel model)
        {
            // 1️⃣ Garantir que o participante está logado
            var participante = await ObterParticipanteLogadoAsync();
            if (participante == null)
            {
                TempData["MensagemErro"] = "Não foi possível carregar o perfil do participante.";
                return RedirectToAction("Index", "Eventos");
            }

            // 2️⃣ Validar model
            if (!ModelState.IsValid)
            {
                await PreencherEventosDisponiveisAsync(model.EventoId);
                return View(model);
            }

            try
            {
                // 3️⃣ Preencher campos obrigatórios
                model.ParticipanteId = participante.Id;
                model.ParticipanteNome = participante.Nome;
                model.DataInscricao ??= DateTime.Now;

                // 4️⃣ Chamar o service
                var created = await _inscricaoService.CreateInscricaoAsync(model);

                if (created == null)
                {
                    TempData["MensagemErro"] = "Falha ao criar inscrição. Verifique os logs para mais detalhes.";
                    await PreencherEventosDisponiveisAsync(model.EventoId);
                    return View(model);
                }

                TempData["MensagemSucesso"] = $"Inscrição criada com sucesso para {created.ParticipanteNome}!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao criar inscrição: {ex.Message}";
                await PreencherEventosDisponiveisAsync(model.EventoId);
                return View(model);
            }
        }


        // GET: Inscricoes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var participante = await ObterParticipanteLogadoAsync();
            if (participante == null)
                return RedirectToAction("Index", "Eventos");

            var todas = await _inscricaoService.GetAllInscricoesAsync();
            var inscricao = todas.FirstOrDefault(i => i.Id == id && i.ParticipanteId == participante.Id);

            if (inscricao == null)
            {
                TempData["MensagemErro"] = "Inscrição não encontrada ou acesso negado.";
                return RedirectToAction(nameof(Index));
            }

            return View(inscricao);
        }
    }
}
