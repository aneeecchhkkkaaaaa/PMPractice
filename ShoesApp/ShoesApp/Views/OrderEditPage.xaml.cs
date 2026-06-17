using ShoesApp.ViewModels;

namespace ShoesApp;

public partial class OrderEditPage : ContentPage
{
    public OrderEditPage()
    {
        InitializeComponent();
        BindingContext = new OrderEditViewModel();
    }
}