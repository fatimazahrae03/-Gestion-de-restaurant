using System;
using Microsoft.Maui.Controls;
using MauiApp2.Services;
using System.Diagnostics;

namespace MauiApp2.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public LoginPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                // Cacher le message d'erreur précédent
                ErrorLabel.IsVisible = false;
                
                // Récupération des données du formulaire
                string usernameOrEmail = UsernameEntry.Text?.Trim();
                string password = PasswordEntry.Text;

                // Validation de base
                if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
                {
                    ShowError("Veuillez remplir tous les champs.");
                    return;
                }

                // Authentification de l'utilisateur
                var user = await _databaseService.AuthenticateUserAsync(usernameOrEmail, password);

                if (user != null)
                {
                    // L'authentification a réussi
                    Debug.WriteLine($"Utilisateur connecté: {user.Username}, Type: {user.UserType}");
                    
                    // Stocker les informations de l'utilisateur dans les préférences de l'application
                    await StoreUserSession(user.Id, user.Username, user.UserType);
                    
                    // Mettre à jour la visibilité des onglets selon le type d'utilisateur
                    if (Shell.Current is AppShell appShell)
                    {
                        appShell.UpdateTabsVisibility();
                    }

                    // Rediriger vers la page d'accueil ou le tableau de bord approprié
                    if (user.UserType == "admin")
                    {
                        // Redirection vers le tableau de bord administrateur
                        await Shell.Current.GoToAsync("//MenuPage");
                    }
                    else
                    {
                        // Redirection vers la page d'accueil du client
                        await Shell.Current.GoToAsync("//Menu");
                    }
                }
                else
                {
                    ShowError("Nom d'utilisateur ou mot de passe incorrect.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la connexion: {ex.Message}");
                ShowError("Une erreur est survenue lors de la connexion. Veuillez réessayer.");
            }
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
        }

        private async void OnRegisterTapped(object sender, EventArgs e)
        {
            // Navigation vers la page d'inscription
            await Shell.Current.GoToAsync("//RegisterPage");
        }
        
        // Méthode pour stocker les informations de session de l'utilisateur
        private async Task StoreUserSession(int userId, string username, string userType)
        {
            try
            {
                Preferences.Set("UserId", userId);
                Preferences.Set("Username", username);
                Preferences.Set("UserType", userType);
                Preferences.Set("IsLoggedIn", true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors du stockage de la session: {ex.Message}");
            }
        }
    }
}