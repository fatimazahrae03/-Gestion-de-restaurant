using MauiApp2.Models;
using System;
using System.Windows.Input;

namespace MauiApp2.Views
{
    public partial class CategoryDetailPage : ContentPage
    {
        public event EventHandler CategorySaved;
        
        public Category Category { get; set; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public string Title { get; set; }
        
        public CategoryDetailPage(Category category)
        {
            InitializeComponent();
            Category = category;
            Title = Category.Id == 0 ? "Ajouter une catégorie" : "Modifier la catégorie";
            
            SaveCommand = new Command(OnSave);
            CancelCommand = new Command(OnCancel);
            
            BindingContext = this;
        }
        
        private async void OnSave()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Category.Name))
                {
                    await DisplayAlert("Erreur", "Le nom de la catégorie est requis", "OK");
                    return;
                }
        
                CategorySaved?.Invoke(this, EventArgs.Empty);
        
                // Il pourrait y avoir un problème ici - ajoutez un délai
                await Task.Delay(100);
        
                await Shell.Current.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur dans OnSave: {ex.Message}");
                await DisplayAlert("Erreur", $"Une erreur est survenue: {ex.Message}", "OK");
            }
        }
        private async void OnCancel()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}