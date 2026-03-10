using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Auth.Core.Interfaces.Services;

namespace Projeto.Moope.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string nome, string email, string password)
        {
            await _userService.CreateUser(nome, email, password);
            return Ok();
        }
    }
}