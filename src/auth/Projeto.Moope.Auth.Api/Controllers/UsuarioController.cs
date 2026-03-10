using Microsoft.AspNetCore.Mvc;

namespace Projeto.Moope.Auth.Api.Controllers
{
    public class UsuarioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

