using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ShoesApp.Infrastructures;
using ShoesApp.Models;

namespace ShoesApp.ViewModels
{
    class AuthViewModel : INotifyPropertyChanged
    {

        DbService _dbService = new DbService();
        private string _login = string.Empty;
        private string _password = string.Empty;
        public string Login { get => _login; set { _login = value; OnPropertyChanged(); } }
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        public ICommand LoginCommand { get; }
        public ICommand LoginAsGuestCommand { get; }

        public AuthViewModel() 
        {
            LoginCommand = new Command(OnLogin);
            LoginAsGuestCommand = new Command(LoginAsGuest);
        }

        public async void OnLogin() 
        {
            var user = await _dbService.GetUser(_login, _password);
            if (user == null || user.Login == null || _login == string.Empty || _password == string.Empty)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Некорректные данные", "OK");
            }
            else
            { 
                var navigationParameter = new Dictionary<string, object>
                {
                    { "CurrentUser", user } 
                };
                Login = string.Empty;
                Password = string.Empty;
                OnPropertyChanged();
                await Shell.Current.GoToAsync(nameof(ProductPage), navigationParameter);
            }
        }
        public async void LoginAsGuest() {
            Login = string.Empty;
            Password = string.Empty;
            OnPropertyChanged();
            await Shell.Current.GoToAsync(nameof(ProductPage));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
