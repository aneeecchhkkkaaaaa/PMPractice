using ShoesApp.ViewModels;

namespace ShoesApp;

public partial class ProductPage : ContentPage
{
    public ProductPage()
    {
        InitializeComponent();
        BindingContext = new ProductViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as ProductViewModel)?.LoadProductsAsync();
    }
}