using System.Collections.ObjectModel;
using MauiApp2.ViewModels;

namespace MauiApp2.Services;

// Nouveau fichier CartService.cs
public class CartService
{
    private static CartService _instance;
    public static CartService Instance => _instance ??= new CartService();
    
    public ObservableCollection<CartItem> Items { get; set; } = new ObservableCollection<CartItem>();
    public int TableNumber { get; set; } = 1;
}