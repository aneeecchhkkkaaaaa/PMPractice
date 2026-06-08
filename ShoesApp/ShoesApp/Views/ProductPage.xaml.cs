using ShoesApp.ViewModels;

namespace ShoesApp;

public partial class ProductPage : ContentPage
{
	public ProductPage()
	{
		InitializeComponent();
		BindingContext = new ProductViewModel();
	}
}