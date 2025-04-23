using Microsoft.Extensions.Logging;
using MauiApp2.Services;
using MauiApp2.ViewModels;
using MauiApp2.Views;

namespace MauiApp2;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Configuration de la chaîne de connexion
        string connectionString = "server=localhost;user=root;password=;database=restaurant_db";
       

        // Enregistrement des services
        builder.Services.AddSingleton(new DatabaseService(connectionString));
        builder.Services.AddSingleton<ReservationService>();
        
        // Enregistrement des ViewModels
      
        builder.Services.AddTransient<OrderViewModel>();
        builder.Services.AddTransient<ReservationViewModel>();
        
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

        // Enregistrement des Pages
        builder.Services.AddTransient<MenuPage>();
        
        builder.Services.AddTransient<MenuViewModel>();
        
        
        // Supprimer l'enregistrement en double de MenuService
        builder.Services.AddSingleton<MenuService>(); // Gardez seulement celui-ci une fois

// Ajoutez l'enregistrement correct pour la page Menu avec son ViewModel
        builder.Services.AddTransient<ClientMenuViewModel>();
        
       
        
// Pareil pour LoginViewModel et LoginPage
        builder.Services.AddTransient<Views.Menu>();

// Assurez-vous que les routes sont enregistrées
        
        
        
        builder.Services.AddSingleton<MenuService>();

        // Enregistrer les ViewModels
        builder.Services.AddTransient<HomeViewModel>();
        
        
        // Enregistrer les Views
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
       
        
        builder.Services.AddTransient<CartPage>();
        builder.Services.AddTransient<CartPageViewModel>();
        builder.Services.AddTransient<Views.PaymentPage>();
        builder.Services.AddTransient<ViewModels.PaymentPageViewModel>();
       
        return builder.Build();
    }
}