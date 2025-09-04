using System.Diagnostics;
using EventosMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventosMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        // P�gina inicial
        public IActionResult Index()
        {
            return View();
        }


    }
}
