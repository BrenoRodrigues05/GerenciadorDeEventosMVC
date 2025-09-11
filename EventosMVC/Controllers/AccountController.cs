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
        private readonly IParticipantesService _participantesService;

        public AccountController(IAutenticação autenticacao, IParticipantesService participantesService)
        {
            _autenticacao = autenticacao;
            _participantesService = participantesService;
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
                // 1️⃣ Autenticar usuário
                var usuario = new UsuarioViewModel { Email = model.Email, Senha = model.Password };
                var token = await _autenticacao.Autenticar(usuario);

                if (token == null || string.IsNullOrEmpty(token.Token))
                {
                    TempData["MensagemErro"] = "Usuário ou senha inválidos";
                    return View(model);
                }

                // 2️⃣ Obter participante pelo email
                var participante = await _participantesService.GetByEmailAsync(usuario.Email, token.Token);
                if (participante == null)
                {
                    TempData["MensagemErro"] = "Não foi possível carregar o perfil do participante.";
                    return View(model);
                }

                // 3️⃣ Criar claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Email),
                    new Claim("JWT", token.Token),
                    new Claim("sub", participante.Id.ToString()) // garante funcionamento do sub
                };

                // Adicionar roles do JWT
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token.Token);
                foreach (var claim in jwt.Claims)
                {
                    if (claim.Type == "role" || claim.Type == "roles" || claim.Type == ClaimTypes.Role)
                        claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                }

                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

                // 4️⃣ Criar cookie de autenticação
                await HttpContext.SignInAsync(
                    "MyCookieAuth",
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(1)
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
                // 1️⃣ Registrar usuário na API de autenticação
                var resultado = await _autenticacao.Registrar(model);
                if (!resultado.Sucesso)
                {
                    TempData["MensagemErro"] = resultado.Mensagem;
                    return View(model);
                }

                // 2️⃣ Login automático
                var usuarioParaLogin = new UsuarioViewModel { Email = model.Email, Senha = model.Senha };
                var token = await _autenticacao.Autenticar(usuarioParaLogin);

                if (token == null || string.IsNullOrEmpty(token.Token))
                {
                    TempData["MensagemErro"] = "Erro ao autenticar após registro.";
                    return View(model);
                }

                // 3️⃣ Criar participante e obter ID real
                var participante = await _participantesService.CreateAsync(new ParticipantesViewModel
                {
                    Nome = model.Nome,
                    Email = model.Email,
                    Telefone = model.Telefone,
                }, token.Token);

                if (participante == null)
                {
                    TempData["MensagemErro"] = "Erro ao criar participante.";
                    return View(model);
                }

                // 4️⃣ Criar claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuarioParaLogin.Email),
                    new Claim("JWT", token.Token),
                    new Claim("sub", participante.Id.ToString())
                };

                // Adicionar roles do JWT
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token.Token);
                foreach (var claim in jwt.Claims)
                {
                    if (claim.Type == "role" || claim.Type == "roles" || claim.Type == ClaimTypes.Role)
                        claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                }

                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

                // 5️⃣ Criar cookie
                await HttpContext.SignInAsync(
                    "MyCookieAuth",
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    });

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
