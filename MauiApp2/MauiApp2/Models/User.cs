// Placer ce fichier dans le dossier Models
namespace MauiApp2.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Ne stockez jamais le mot de passe en clair dans la mémoire de l'application en production
        public string Email { get; set; }
        public string FullName { get; set; }
        public string UserType { get; set; } // "admin" ou "client"
        public DateTime CreatedAt { get; set; }
    }
}