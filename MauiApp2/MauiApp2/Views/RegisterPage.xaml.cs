using System;
using Microsoft.Maui.Controls;
using MauiApp2.Models;
using MauiApp2.Services;
using System.Diagnostics;

namespace MauiApp2.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public RegisterPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            
            // Définir l'index par défaut sur Client (index 0)
            UserTypePicker.SelectedIndex = 0;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            try
            {
                // Cacher le message d'erreur précédent
                ErrorLabel.IsVisible = false;
                
                // Récupération des données du formulaire
                string username = UsernameEntry.Text?.Trim();
                string email = EmailEntry.Text?.Trim();
                string fullName = FullNameEntry.Text?.Trim();
                string password = PasswordEntry.Text;
                string confirmPassword = ConfirmPasswordEntry.Text;
                string userType = UserTypePicker.SelectedItem?.ToString()?.ToLower();

                // Validation de base
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ShowError("Veuillez remplir tous les champs obligatoires.");
                    return;
                }

                // Validation de l'email
                if (!IsValidEmail(email))
                {
                    ShowError("Veuillez entrer une adresse email valide.");
                    return;
                }

                // Vérification de la correspondance des mots de passe
                if (password != confirmPassword)
                {
                    ShowError("Les mots de passe ne correspondent pas.");
                    return;
                }

                // Vérification de la force du mot de passe
                if (password.Length < 6)
                {
                    ShowError("Le mot de passe doit contenir au moins 6 caractères.");
                    return;
                }

                // Mapping userType
                if (userType == "client")
                {
                    userType = "client";
                }
                else if (userType == "admin")
                {
                    userType = "admin";
                }
                else
                {
                    // Valeur par défaut si aucune n'est sélectionnée
                    userType = "client";
                }

                // Création d'un nouvel utilisateur
                var user = new User
                {
                    Username = username,
                    Email = email,
                    Password = password,  // Le hachage est géré dans RegisterUserAsync
                    FullName = fullName,
                    UserType = userType
                };

                // Enregistrement de l'utilisateur
                bool success = await _databaseService.RegisterUserAsync(user);

                if (success)
                {
                    // Afficher un message de confirmation
                    await DisplayAlert("Succès", "Votre compte a été créé avec succès!", "OK");
                    
                    // Rediriger vers la page de connexion
                    await Shell.Current.GoToAsync("//LoginPage");
                }
                else
                {
                    ShowError("L'inscription a échoué. Ce nom d'utilisateur ou cette adresse email existe peut-être déjà.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de l'inscription: {ex.Message}");
                ShowError("Une erreur est survenue lors de l'inscription. Veuillez réessayer.");
            }
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async void OnLoginTapped(object sender, EventArgs e)
        {
            // Navigation vers la page de connexion
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }
}