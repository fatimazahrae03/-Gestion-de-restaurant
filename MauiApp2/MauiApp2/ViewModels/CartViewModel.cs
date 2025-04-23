using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using MauiApp2.Models;
using MauiApp2.Services;

namespace MauiApp2.ViewModels
{
    [QueryProperty(nameof(CartItems), "CartItems")]
    [QueryProperty(nameof(TableNumber), "TableNumber")]
    public class CartPageViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<CartItem> _cartItems;
        private int _tableNumber;
        private decimal _totalAmount;
        private string _specialRequests;

        public CartPageViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _cartItems = new ObservableCollection<CartItem>();

            PlaceOrderCommand = new Command(async () => await PlaceOrderAsync());
            RemoveItemCommand = new Command<CartItem>(RemoveItem);
        }

        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set
            {
                if (SetProperty(ref _cartItems, value))
                {
                    CalculateTotalAmount();
                }
            }
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

        public ICommand PlaceOrderCommand { get; }
        public ICommand RemoveItemCommand { get; }

        private void CalculateTotalAmount()
        {
            TotalAmount = CartItems.Sum(item => item.Total);
        }

        private void RemoveItem(CartItem item)
        {
            if (item != null)
            {
                CartItems.Remove(item);
                CalculateTotalAmount();
            }
        }

        private async Task PlaceOrderAsync()
        {
            if (CartItems.Count == 0)
            {
                await Shell.Current.DisplayAlert("Panier vide",
                    "Ajoutez des articles à votre panier avant de passer commande.", "OK");
                return;
            }

            try
            {
                // Préparer les paramètres pour la page de paiement
                var parameters = new Dictionary<string, object>
                {
                    { "CartItems", CartItems },
                    { "TableNumber", TableNumber },
                    { "TotalAmount", TotalAmount },
                    { "SpecialRequests", SpecialRequests ?? string.Empty }
                };

                // Naviguer vers la page de paiement
                await Shell.Current.GoToAsync("PaymentPage", parameters);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la navigation vers la page de paiement: {ex.Message}");
                await Shell.Current.DisplayAlert("Erreur", "Une erreur est survenue lors de la navigation vers la page de paiement.", "OK");
            }
        }
    }
}