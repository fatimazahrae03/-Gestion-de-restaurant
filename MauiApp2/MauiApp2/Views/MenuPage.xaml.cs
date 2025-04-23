using MauiApp2.ViewModels;

namespace MauiApp2.Views
{
    public partial class MenuPage : ContentPage
    {
        private readonly MenuViewModel _viewModel;
        
        public MenuPage(MenuViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadCategoriesCommand.Execute(null);
        }
    }
}