using EventosMVC.Models;
using EventosMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventosMVC.Controllers
{
    public class EventosController : Controller
    {
        private readonly IEventosService _eventosService;

        public EventosController(IEventosService eventosService)
        {
            _eventosService = eventosService;
        }

        public async Task<IActionResult> Index()
        {
            var eventos = await _eventosService.GetAllEventosAsync() ?? new List<EventosViewModel>();
            return View(eventos);
        }

        public async Task<IActionResult> Details(int id)
        {
            var evento = await _eventosService.GetEventoByIdAsync(id);
            if (evento == null)
            {
                TempData["MensagemErro"] = "Evento não encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(evento);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventosViewModel evento)
        {
            if (!ModelState.IsValid)
                return View(evento);

            var (Sucesso, Mensagem) = await _eventosService.CreateEventoAsync(evento);

            if (Sucesso)
            {
                TempData["MensagemSucesso"] = Mensagem;
                return View(evento);
            }

            TempData["MensagemErro"] = Mensagem;
            return View(evento);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var evento = await _eventosService.GetEventoByIdAsync(id);
            if (evento == null)
            {
                TempData["MensagemErro"] = "Evento não encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(evento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventosViewModel evento)
        {
            if (id != evento.Id)
            {
                TempData["MensagemErro"] = "ID inválido.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
                return View(evento);

            var (Sucesso, Mensagem) = await _eventosService.UpdateEventoAsync(id, evento);

            if (Sucesso)
            {
                TempData["MensagemSucesso"] = Mensagem;
                return View(evento);
            }

            TempData["MensagemErro"] = Mensagem;
            return View(evento);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var evento = await _eventosService.GetEventoByIdAsync(id);
            if (evento == null)
            {
                TempData["MensagemErro"] = "Evento não encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(evento);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (Sucesso, Mensagem) = await _eventosService.DeleteEventoAsync(id);

            if (Sucesso)
            {
                TempData["MensagemSucesso"] = Mensagem;
                return RedirectToAction(nameof(Index));
            }

            TempData["MensagemErro"] = Mensagem;
            return RedirectToAction(nameof(Index));
        }
    }
}
