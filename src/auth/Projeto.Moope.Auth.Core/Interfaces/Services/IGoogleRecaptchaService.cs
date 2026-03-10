namespace Projeto.Moope.Auth.Core.Interfaces.Services
{
    public interface IGoogleRecaptchaService
    {
        Task<bool> VerifyTokenAsync(string token);
    }
}
