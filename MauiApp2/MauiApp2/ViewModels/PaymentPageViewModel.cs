using MauiApp2.Services;



using System.Windows.Input;
using MauiApp2.Models;
using MauiApp2.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MauiApp2.ViewModels
{
    [QueryProperty(nameof(CartItems), "CartItems")]
    [QueryProperty(nameof(TableNumber), "TableNumber")]
    [QueryProperty(nameof(TotalAmount), "TotalAmount")]
    [QueryProperty(nameof(SpecialRequests), "SpecialRequests")]
    public class PaymentPageViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<CartItem> _cartItems;
        private int _tableNumber;
        private decimal _totalAmount;
        private string _specialRequests;
        private bool _isProcessing;

        public PaymentPageViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _cartItems = new ObservableCollection<CartItem>();
            
            ProcessPaymentCommand = new Command(async () => await ProcessPaymentAsync(), () => !IsProcessing);
            CancelPaymentCommand = new Command(async () => await CancelPaymentAsync(), () => !IsProcessing);
        }
        
        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set => SetProperty(ref _cartItems, value);
        }
        
        public int TableNumber
        {
            get => _tableNumber;
            set => SetProperty(ref _tableNumber, value);
        }
        
        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }
        
        public string SpecialRequests
        {
            get => _specialRequests;
            set => SetProperty(ref _specialRequests, value);
        }
        
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (SetProperty(ref _isProcessing, value))
                {
                    (ProcessPaymentCommand as Command)?.ChangeCanExecute();
                    (CancelPaymentCommand as Command)?.ChangeCanExecute();
                }
            }
        }
        
        public ICommand ProcessPaymentCommand { get; }
        public ICommand CancelPaymentCommand { get; }
        
        private async Task ProcessPaymentAsync()
        {
            if (CartItems.Count == 0)
            {
                await Shell.Current.DisplayAlert("Erreur", "Votre panier est vide.", "OK");
                return;
            }
            
            try
            {
                IsProcessing = true;
                
                // Simuler un traitement de paiement
                await Shell.Current.DisplayAlert("Traitement", "Paiement en cours...", "OK");
                
                // Attendre de façon simulée pour donner l'impression d'un traitement
                await Task.Delay(1500);
                
                // Récupérer l'ID de l'utilisateur connecté
                int userId = Preferences.Get("UserId", 0);
                
                if (userId == 0)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Vous devez être connecté pour finaliser la commande.", "OK");
                    await Shell.Current.GoToAsync("//LoginPage");
                    return;
                }
                
                // Créer une nouvelle commande
                int orderId = await _databaseService.CreateOrderAsync(TableNumber, userId);
                
                if (orderId <= 0)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Impossible de créer la commande.", "OK");
                    return;
                }
                
                // Ajouter les détails des articles à la commande
                var cartItemsList = CartItems.ToList();
                
                // Appliquer les requêtes spéciales si nécessaire
                if (!string.IsNullOrEmpty(SpecialRequests))
                {
                    foreach (var item in cartItemsList)
                    {
                        item.SpecialRequests = item.SpecialRequests ?? string.Empty;
                        if (!string.IsNullOrEmpty(item.SpecialRequests))
                        {
                            item.SpecialRequests += "\n";
                        }
                        
                        item.SpecialRequests += SpecialRequests;
                    }
                }
                
                bool success = await _databaseService.AddOrderItemsAsync(orderId, cartItemsList);
                
                if (success)
                {
                    await Shell.Current.DisplayAlert("Succès", "Paiement accepté ! Votre commande a été passée avec succès.", "OK");
                    
                    // Vider le panier et retourner à la page du menu
                    CartItems.Clear();
                    await Shell.Current.GoToAsync("//Menu");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erreur", "Une erreur est survenue lors de l'enregistrement de la commande.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors du traitement du paiement: {ex.Message}");
                await Shell.Current.DisplayAlert("Erreur", "Une erreur est survenue lors du traitement du paiement.", "OK");
            }
            finally
            {
                IsProcessing = false;
            }
        }
        
        private async Task CancelPaymentAsync()
        {
            bool cancel = await Shell.Current.DisplayAlert("Annuler", "Êtes-vous sûr de vouloir annuler le paiement ?", "Oui", "Non");
            
            if (cancel)
            {
                // Retourner à la page du panier
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}