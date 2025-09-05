using EventosMVC.Models;
using EventosMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventosMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAutenticação _autenticacao;

        public AccountController(IAutenticação autenticacao)
        {
            _autenticacao = autenticacao;
        }

        #region LOGIN

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var usuario = new UsuarioViewModel
                {
                    Email = model.Email,
                    Senha = model.Password
                };

                var token = await _autenticacao.Autenticar(usuario);

                if (token == null || string.IsNullOrEmpty(token.Token))
                {
                    TempData["MensagemErro"] = "Usuário ou senha inválidos";
                    return View(model);
                }

                Response.Cookies.Append("AuthToken", token.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = model.RememberMe
                        ? DateTimeOffset.UtcNow.AddDays(7)
                        : DateTimeOffset.UtcNow.AddHours(1)
                });

                TempData["MensagemSucesso"] = "Login realizado com sucesso!";
                return RedirectToAction("Index", "Home"); // Vai para a Home
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao autenticar: {ex.Message}";
                return View(model);
            }
        }

        #endregion

        #region LOGOUT

        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            TempData["MensagemSucesso"] = "Logout realizado com sucesso!";
            return RedirectToAction("Login");
        }

        #endregion

        #region REGISTRO

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegistroViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var resultado = await _autenticacao.Registrar(model);

                if (!resultado.Sucesso)
                {
                    TempData["MensagemErro"] = resultado.Mensagem;
                    return View(model);
                }

                // Loga automaticamente após registrar
                var usuarioParaLogin = new UsuarioViewModel
                {
                    Email = model.Email,
                    Senha = model.Senha
                };

                var token = await _autenticacao.Autenticar(usuarioParaLogin);

                if (token != null && !string.IsNullOrEmpty(token.Token))
                {
                    Response.Cookies.Append("AuthToken", token.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });
                }

                TempData["MensagemSucesso"] = "Conta registrada e login realizado com sucesso!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao registrar: {ex.Message}";
                return View(model);
            }
        }

        #endregion
    }
}
