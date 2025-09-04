using EventosMVC.Models;

namespace EventosMVC.Services
{
    public interface IAutenticação
    {
        Task <TokenViewModel> Autenticar(UsuarioViewModel usuario);

        Task<ResultadoViewModel> Registrar(RegistroViewModel usuario);
    }
}
