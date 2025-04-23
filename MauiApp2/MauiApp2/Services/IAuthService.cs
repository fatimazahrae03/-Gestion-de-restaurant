using MauiApp2.Models;

namespace MauiApp2.Services
{
    public interface IAuthService
    {
        User GetCurrentUser();
        Task<bool> LoginAsync(string usernameOrEmail, string password);
        void Logout();
        bool IsLoggedIn();
    }
}