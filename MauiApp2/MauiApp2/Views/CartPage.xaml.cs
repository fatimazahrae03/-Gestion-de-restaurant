
using System.Diagnostics;
using MauiApp2.ViewModels;
namespace MauiApp2.Views;
public partial class CartPage : ContentPage
{
    public CartPage(CartPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        Debug.WriteLine("CartPage initialisée avec ViewModel");
    }

    
}