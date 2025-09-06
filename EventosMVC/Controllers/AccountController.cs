using EventosMVC.Models;
using EventosMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

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

                // Extrair claims do JWT
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token.Token);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Email),
                    new Claim("JWT", token.Token) // Salva o JWT como claim
                };

                // Adiciona roles do JWT
                foreach (var claim in jwt.Claims)
                {
                    if (claim.Type == "role" || claim.Type == "roles" || claim.Type == ClaimTypes.Role)
                        claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                }

                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

                await HttpContext.SignInAsync(
                    "MyCookieAuth",
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe
                            ? DateTimeOffset.UtcNow.AddDays(7)
                            : DateTimeOffset.UtcNow.AddHours(1)
                    });

                TempData["MensagemSucesso"] = "Login realizado com sucesso!";
                return RedirectToAction("Index", "Home");
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
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            TempData["MensagemSucesso"] = "Logout realizado com sucesso!";
            return RedirectToAction("Login");
        }

        #endregion

        #region REGISTRO

        [HttpGet]
        public IActionResult Register() => View(new RegistroViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistroViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var resultado = await _autenticacao.Registrar(model);

                if (!resultado.Sucesso)
                {
                    TempData["MensagemErro"] = resultado.Mensagem;
                    return View(model);
                }

                // Login automático após registrar
                var usuarioParaLogin = new UsuarioViewModel
                {
                    Email = model.Email,
                    Senha = model.Senha
                };
                var token = await _autenticacao.Autenticar(usuarioParaLogin);

                if (token != null && !string.IsNullOrEmpty(token.Token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token.Token);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuarioParaLogin.Email),
                        new Claim("JWT", token.Token) // Salva o JWT como claim
                    };

                    // Adiciona roles
                    foreach (var claim in jwt.Claims)
                    {
                        if (claim.Type == "role" || claim.Type == "roles" || claim.Type == ClaimTypes.Role)
                            claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

                    await HttpContext.SignInAsync(
                        "MyCookieAuth",
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
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
