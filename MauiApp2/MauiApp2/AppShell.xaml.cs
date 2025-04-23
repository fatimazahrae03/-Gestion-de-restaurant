using System.Diagnostics;
using MauiApp2.Services;
using MauiApp2.Views; // Ajoutez cette ligne pour accéder à vos vues

namespace MauiApp2;

public partial class AppShell : Shell
{
    private readonly DatabaseService _databaseService;

    public AppShell(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        Navigated += OnNavigated;
        
        // Enregistrez votre route ici
        Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
        
        // Enregistrez aussi les routes pour les pages de détail
        Routing.RegisterRoute(nameof(CategoryDetailPage), typeof(CategoryDetailPage));
        Routing.RegisterRoute(nameof(MenuItemDetailPage), typeof(MenuItemDetailPage));
        Routing.RegisterRoute(nameof(Views.Menu), typeof(Views.Menu));
      
        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        // Dans la méthode constructeur de AppShell
        Routing.RegisterRoute("CartPage", typeof(CartPage));
        Routing.RegisterRoute("CartPage", typeof(Views.CartPage));
        Routing.RegisterRoute("PaymentPage", typeof(Views.PaymentPage));
        
        Loaded += AppShell_Loaded;
    }

    private async void AppShell_Loaded(object sender, EventArgs e)
    {
        await InitializeApp();
    }

    private async Task InitializeApp()
    {
        try
        {
            bool isConnected = await _databaseService.TestConnectionAsync();
            if (isConnected)
            {
                Debug.WriteLine("Connexion à la base de données réussie!");
                await _databaseService.InitializeDatabaseAsync();
                await DisplayAlert("Connexion réussie", "La base de données est connectée et initialisée!", "OK");
            }
            else
            {
                Debug.WriteLine("Échec de la connexion à la base de données");
                await DisplayAlert("Erreur", "Impossible de se connecter à la base de données", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception: {ex.Message}");
            await DisplayAlert("Erreur", $"Une erreur s'est produite: {ex.Message}", "OK");
        }
    }
    private void OnNavigated(object sender, ShellNavigatedEventArgs e)
    {
        // Mettre à jour la visibilité des onglets après chaque navigation
        UpdateTabsVisibility();
    }
    public void UpdateTabsVisibility()
    {
        // Vérifier si l'utilisateur est connecté
        bool isLoggedIn = Preferences.Get("IsLoggedIn", false);
    
        // Vérifier le type d'utilisateur (admin ou client)
        string userType = Preferences.Get("UserType", string.Empty);
    
        // Vérifier si l'utilisateur est un administrateur
        bool isAdmin = userType.Equals("admin", StringComparison.OrdinalIgnoreCase);
    
        // Mettre à jour la visibilité des onglets d'administration
        MenuManagementTab.IsVisible = isAdmin;
        OrdersTab.IsVisible = isAdmin;
        ReservationsTab.IsVisible = isAdmin;
    
        // Mettre à jour la visibilité de l'onglet Menu (visible seulement pour les clients)
        MenuTab.IsVisible = !isAdmin;
    }
}