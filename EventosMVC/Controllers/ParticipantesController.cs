using EventosMVC.Models;
using EventosMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosMVC.Controllers
{
    [Authorize] // Apenas usuários logados
    public class ParticipantesController : Controller
    {
        private readonly IParticipantesService _participantesService;

        public ParticipantesController(IParticipantesService participantesService)
        {
            _participantesService = participantesService;
        }

        // 🔹 Exibe os dados do participante logado
        public async Task<IActionResult> Index()
        {
            var participanteId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var participante = await _participantesService.GetByIdAsync(participanteId);

            if (participante == null)
            {
                TempData["MensagemErro"] = "Não foi possível carregar os dados do participante.";
                return RedirectToAction("Index", "Home");
            }

            return View(participante);
        }

        // 🔹 Tela de edição do perfil
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var participanteId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var participante = await _participantesService.GetByIdAsync(participanteId);

            if (participante == null)
            {
                TempData["MensagemErro"] = "Participante não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(participante);
        }

        // 🔹 Salva alterações no perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ParticipantesViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var participanteId = int.Parse(User.FindFirst("sub")?.Value ?? "0");

                if (model.Id != participanteId)
                {
                    TempData["MensagemErro"] = "Não é permitido editar outro participante.";
                    return RedirectToAction(nameof(Index));
                }

                await _participantesService.UpdateAsync(participanteId, model);

                TempData["MensagemSucesso"] = "Perfil atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao atualizar perfil: {ex.Message}";
                return View(model);
            }
        }
    }
}
