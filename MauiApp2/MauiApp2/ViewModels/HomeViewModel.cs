using System.Windows.Input;
using MauiApp2.Views;

namespace MauiApp2.ViewModels
{
    public class HomeViewModel
    {
        public ICommand LoginCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }

        public HomeViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
            RegisterCommand = new Command(OnRegisterClicked);
        }

        private async void OnLoginClicked()
        {
            // Navigation vers la page de connexion
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }

        private async void OnRegisterClicked()
        {
            try
            {
                // Navigation vers la page d'inscription
                await Shell.Current.GoToAsync(nameof(RegisterPage));
            }
            catch (Exception ex)
            {
                // Affichez l'erreur ou enregistrez-la pour le débogage
                Console.WriteLine($"Erreur de navigation: {ex.Message}");
                // Vous pourriez aussi afficher l'erreur dans un dialogue
            }
            
        }
    }
}