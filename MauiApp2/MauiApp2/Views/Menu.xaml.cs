using MauiApp2.ViewModels;
using System.Diagnostics;

namespace MauiApp2.Views
{
    public partial class Menu : ContentPage
    {
        public Menu(ClientMenuViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void OnImageSizeChanged(object sender, EventArgs e)
        {
            if (sender is Image image)
            {
                // Récupère l'élément de menu associé à cette image
                if (image.BindingContext is MenuItemWithQuantity menuItem)
                {
                    Debug.WriteLine($"Image pour {menuItem.Name}: Source={image.Source}, Loaded={image.IsLoaded}, Width={image.Width}, Height={image.Height}");
                    
                    // Si l'image a échoué à charger
                    if (!image.IsLoaded || image.Width <= 0 || image.Height <= 0)
                    {
                        Debug.WriteLine($"Échec de chargement de l'image pour {menuItem.Name}. Chemin: {menuItem.ImagePath}");
                    }
                }
                else
                {
                    Debug.WriteLine($"Image sans contexte de menu: Source={image.Source}, Loaded={image.IsLoaded}");
                }
            }
        }
    }
}