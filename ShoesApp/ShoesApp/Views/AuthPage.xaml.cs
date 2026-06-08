using ShoesApp.ViewModels;

namespace ShoesApp;

public partial class AuthPage : ContentPage
{
	public AuthPage()
	{
		InitializeComponent();
		BindingContext = new AuthViewModel();
	}
}