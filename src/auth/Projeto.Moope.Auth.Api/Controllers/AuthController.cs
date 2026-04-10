using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Auth.Api.Services;
using Projeto.Moope.Auth.Api.Utils;
using Projeto.Moope.Auth.Core.DTOs.Login;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Interfaces.Services;
using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Projeto.Moope.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : MainController
    {
        private readonly SignInManager<IdentityUser<Guid>> _signInManager;
        private readonly UserManager<IdentityUser<Guid>> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IJwtSigningKeyProvider _jwtSigningKeys;
        private readonly ILogger _logger;
        private readonly IGoogleRecaptchaService _recaptchaService;
        private readonly IPapelRepository _papelRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        private string[] ErrorPassowrd = { "PasswordTooShort", "PasswordRequiresNonAlphanumeric", "PasswordRequiresLower", "PasswordRequiresUpper", "PasswordRequiresDigit" };
        private string[] ErrorEmail = { "DuplicateUserName" };

        public AuthController(
            SignInManager<IdentityUser<Guid>> signInManager,
            UserManager<IdentityUser<Guid>> userManager,
            IOptions<JwtSettings> config,
            IJwtSigningKeyProvider jwtSigningKeys,
            ILogger<AuthController> logger,
            IGoogleRecaptchaService recaptchaService,
            INotificador notificador,
            IPapelRepository papelRepository,
            IRefreshTokenRepository refreshTokenRepository) : base(notificador)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtSettings = config.Value;
            _jwtSigningKeys = jwtSigningKeys;
            _logger = logger;
            _recaptchaService = recaptchaService;
            _papelRepository = papelRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            //var isValid = await _recaptchaService.VerifyTokenAsync(loginDto.RecaptchaToken);
            //if (!isValid)
            //{
            //    ModelState.AddModelError("Senha", "O captcha está inválido.");
            //    return CustomResponse(ModelState);
            //}

            var result = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Senha, false, true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                var tiposUsuario = await VerificarTiposUsuarioAsync(user.Id);
                if (tiposUsuario.Count() > 1)
                {
                    if (loginDto.TipoUsuario == null)
                    {
                        var response = new LoginMultiploTiposDto
                        {
                            Message = "Usuário possui múltiplos tipos cadastrados. Selecione o tipo de usuário para continuar.",
                            TiposUsuario = tiposUsuario,
                            Email = user.Email
                        };

                        return CustomResponse(response);
                    }
                    else
                    {
                        if (!tiposUsuario.Contains<TipoUsuario>((TipoUsuario)loginDto.TipoUsuario))
                        {
                            ModelState.AddModelError("TipoUsuario", "O tipo de usuário selecionado não está disponível para este usuário.");
                            return CustomResponse(ModelState);
                        }
                    }
                }

                var token = await GerarJwt(loginDto.Email, loginDto.TipoUsuario ?? tiposUsuario.FirstOrDefault());
                return CustomResponse(new { data = token });
            }
            else if (result.IsLockedOut)
            {
                ModelState.AddModelError("Senha", "Usuário temporariamente bloqueado por tentativas inválidas.");
                return CustomResponse(ModelState);
            }
            else if (!result.Succeeded)
            {
                ModelState.AddModelError("Senha", "E-mail ou senha inválidos.");
                return CustomResponse(ModelState);
            }
            else
            {
                ModelState.AddModelError("Senha", "E-mail ou senha inválidos.");
                return CustomResponse(ModelState);
            }
        }

        private async Task<IEnumerable<TipoUsuario>> VerificarTiposUsuarioAsync(Guid userId)
        {
            var tipos = new List<TipoUsuario>();
            var papeis = await _papelRepository.BuscarPorUsuarioIdAsync(userId);

            foreach (var p in papeis)
                tipos.Add(p.TipoUsuario);

            return tipos;
        }

        private async Task<LoginResponseDto> GerarJwt(string email, TipoUsuario? tipoUsuario = null)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            // Se não foi passado o tipo de usuário, tentar obter da primeira role
            if (tipoUsuario == null)
            {
                var role = userRoles.FirstOrDefault();
                if (Enum.TryParse<TipoUsuario>(role, out var parsedTipo))
                {
                    tipoUsuario = parsedTipo;
                }
            }

            var creds = _jwtSigningKeys.GetSigningCredentials();
            var expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiracaoHoras);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            if (tipoUsuario != null)
            {
                claims.Add(new Claim("perfil", tipoUsuario.ToString().ToLower()));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);
            var token = new SecurityTokenDescriptor
            {
                Subject = identityClaims,
                Expires = expires,
                SigningCredentials = creds,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(token);
            var tokenString = tokenHandler.WriteToken(securityToken);

            var refreshToken = await GerarRefreshTokenAsync(user.Id);
            var refreshTokenExpiresIn = TimeSpan.FromDays(_jwtSettings.ExpiracaoRefreshTokenDias).TotalSeconds;

            var loginResponseDto = new LoginResponseDto
            {
                AccessToken = tokenString,
                ExpiresIn = TimeSpan.FromHours(_jwtSettings.ExpiracaoHoras).TotalSeconds,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresIn = refreshTokenExpiresIn,
                User = new LoginUsuarioDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    Claims = claims.Select(c => new ClaimDto { Type = c.Type, Value = c.Value }),
                    Perfil = tipoUsuario?.ToString().ToLower()
                }
            };

            return loginResponseDto;
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            {
                ModelState.AddModelError("RefreshToken", "O Refresh Token é obrigatório");
                return CustomResponse(ModelState);
            }

            var storedToken = await _refreshTokenRepository.BuscarPorTokenAsync(dto.RefreshToken);
            if (storedToken == null)
            {
                ModelState.AddModelError("RefreshToken", "Refresh Token inválido");
                return CustomResponse(ModelState);
            }

            if (!storedToken.IsActive)
            {
                ModelState.AddModelError("RefreshToken", "Refresh Token expirado ou revogado");
                return CustomResponse(ModelState);
            }

            var user = await _userManager.FindByIdAsync(storedToken.UsuarioId.ToString());
            if (user == null)
            {
                ModelState.AddModelError("RefreshToken", "Usuário não encontrado");
                return CustomResponse(ModelState);
            }

            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.ReplacedByToken = null;
            await _refreshTokenRepository.AtualizarAsync(storedToken);
            await _refreshTokenRepository.UnitOfWork.Commit();

            var tiposUsuario = await VerificarTiposUsuarioAsync(user.Id);
            var tipoUsuario = tiposUsuario.FirstOrDefault();
            var loginResponse = await GerarJwt(user.Email!, tipoUsuario);

            return CustomResponse(new { data = loginResponse });
        }

        private async Task<RefreshToken> GerarRefreshTokenAsync(Guid usuarioId)
        {
            var refreshToken = new RefreshToken
            {
                UsuarioId = usuarioId,
                Token = GerarTokenAleatorio(),
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.ExpiracaoRefreshTokenDias),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.RevogarTokensPorUsuarioAsync(usuarioId);
            await _refreshTokenRepository.SalvarAsync(refreshToken);
            await _refreshTokenRepository.UnitOfWork.Commit();

            return refreshToken;
        }

        private static string GerarTokenAleatorio()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}
