// Interface

using MauiApp2.Models;



// Implémentation
namespace MauiApp2.Services
{
    public class AuthService : IAuthService
    {
        private User _currentUser;
        private readonly DatabaseService _databaseService;
        
        public AuthService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        
        public User GetCurrentUser() => _currentUser;
        
        public async Task<bool> LoginAsync(string usernameOrEmail, string password)
        {
            _currentUser = await _databaseService.AuthenticateUserAsync(usernameOrEmail, password);
            return _currentUser != null;
        }
        
        public void Logout()
        {
            _currentUser = null;
        }
        
        public bool IsLoggedIn() => _currentUser != null;
    }
}