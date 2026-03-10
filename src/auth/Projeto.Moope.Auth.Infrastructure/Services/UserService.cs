using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Interfaces.Services;
using Projeto.Moope.Auth.Core.Models;

namespace Projeto.Moope.Auth.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        //private readonly IMessageBus _messageBus;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task CreateUser(string nome, string email, string password)
        {
            // Check if user exists
            var existingUser = await _userRepository.GetByEmail(email);
            if (existingUser != null)
                throw new InvalidOperationException("User already exists");

            // Create user (simplified, add password hashing, etc.)
            var user = new Usuario
            {
                Nome = nome,
                Email = email, // Assuming added
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            await _userRepository.Add(user);

            //// Publish event
            //var message = new UserCreatedMessage { UserId = user.Id, Nome = user.Nome, Email = user.Email };
            //await _messageBus.Publish(message);
        }
    }
}