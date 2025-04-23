using MauiApp2.ViewModels;

namespace MauiApp2.Views
{
    public partial class PaymentPage : ContentPage
    {
        public PaymentPage(PaymentPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}