using ShoesApp.ViewModels;

namespace ShoesApp;

public partial class OrderListPage : ContentPage
{
    public OrderListPage()
    {
        InitializeComponent();
        BindingContext = new OrderListViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as OrderListViewModel)?.LoadOrdersCommand?.Execute(null);
    }
}