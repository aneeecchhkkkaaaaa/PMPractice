using ShoesApp.ViewModels;

namespace ShoesApp;

public partial class ProductEditPage : ContentPage
{
    public ProductEditPage()
    {
        InitializeComponent();
        BindingContext = new ProductEditViewModel();
    }
}