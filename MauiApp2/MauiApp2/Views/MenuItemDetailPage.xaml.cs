using MauiApp2.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MenuItem = MauiApp2.Models.MenuItem;

namespace MauiApp2.Views
{
    public partial class MenuItemDetailPage : ContentPage
    {
        public event EventHandler MenuItemSaved;
        
        public MenuItem MenuItem { get; set; }
        public ObservableCollection<Category> Categories { get; set; }
        public Category SelectedCategory { get; set; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseImageCommand { get; }
        public string Title { get; set; }

        public MenuItemDetailPage(MenuItem menuItem, ObservableCollection<Category> categories)
        {
            InitializeComponent();
            MenuItem = menuItem;
            Categories = categories;
            Title = MenuItem.Id == 0 ? "Ajouter un plat" : "Modifier le plat";
            
            // Find the selected category
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == MenuItem.CategoryId);
            
            SaveCommand = new Command(OnSave);
            CancelCommand = new Command(OnCancel);
            BrowseImageCommand = new Command(OnBrowseImage);
            
            BindingContext = this;
        }

        private async void OnSave()
        {
            if (string.IsNullOrWhiteSpace(MenuItem.Name))
            {
                await DisplayAlert("Erreur", "Le nom du plat est requis", "OK");
                return;
            }

            if (MenuItem.Price <= 0)
            {
                await DisplayAlert("Erreur", "Le prix doit être supérieur à 0", "OK");
                return;
            }

            if (SelectedCategory != null)
            {
                MenuItem.CategoryId = SelectedCategory.Id;
            }

            MenuItemSaved?.Invoke(this, EventArgs.Empty);
            await Shell.Current.Navigation.PopModalAsync();
        }

        private async void OnCancel()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private async void OnBrowseImage()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    MenuItem.ImagePath = result.FullPath;
                    OnPropertyChanged(nameof(MenuItem));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible de sélectionner l'image: {ex.Message}", "OK");
            }
        }
    }
}