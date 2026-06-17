using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ShoesApp.Infrastructures;
using ShoesApp.Models;

namespace ShoesApp.ViewModels
{
    [QueryProperty(nameof(CurrentUser), "CurrentUser")]
    [QueryProperty(nameof(OrderToEdit), "OrderToEdit")]
    [QueryProperty(nameof(IsEditMode), "IsEditMode")]
    public class OrderEditViewModel : INotifyPropertyChanged
    {
        private readonly DbService _dbService = new();
        private User _currentUser;
        private Order _order = new();
        private bool _isEditMode;
        private List<Address> _addresses = new();
        private List<OrderStatus> _statuses = new();
        private Address _selectedAddress;
        private OrderStatus _selectedStatus;
        private bool _isLoaded = false;

        public Order Order
        {
            get => _order;
            set { _order = value; OnPropertyChanged(); }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                if (!_isLoaded && _currentUser != null)
                    LoadLists();
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(Title)); }
        }

        public string Title => IsEditMode ? "Редактирование заказа" : "Новый заказ";

        public List<Address> Addresses
        {
            get => _addresses;
            set { _addresses = value; OnPropertyChanged(); }
        }

        public List<OrderStatus> Statuses
        {
            get => _statuses;
            set { _statuses = value; OnPropertyChanged(); }
        }

        public Address SelectedAddress
        {
            get => _selectedAddress;
            set { _selectedAddress = value; if (value != null) Order.AddressId = value.AddressId; OnPropertyChanged(); }
        }

        public OrderStatus SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; if (value != null) Order.StatusId = value.OrderStatusId; OnPropertyChanged(); }
        }

        public Order OrderToEdit
        {
            set
            {
                if (value != null)
                {
                    Order = value;
                    IsEditMode = true;
                    if (!_isLoaded)
                        LoadLists();
                    else
                        SetSelectedItems();
                }
            }
        }

        public ICommand SaveCommand { get; }

        public OrderEditViewModel()
        {
            SaveCommand = new Command(async () => await OnSave());
        }

        private async void LoadLists()
        {
            _isLoaded = true;
            Addresses = await _dbService.GetAddresses();
            Statuses = await _dbService.GetOrderStatuses();
            SetSelectedItems();
        }

        private void SetSelectedItems()
        {
            if (!Addresses.Any() || !Statuses.Any())
                return;

            if (IsEditMode)
            {
                SelectedAddress = Addresses.FirstOrDefault(a => a.AddressId == Order.AddressId);
                SelectedStatus = Statuses.FirstOrDefault(s => s.OrderStatusId == Order.StatusId);
            }
            else
            {
                SelectedAddress = Addresses.First();
                SelectedStatus = Statuses.First();
                // Убедимся, что даты есть
                if (Order.OrderDate == default(DateOnly))
                    Order.OrderDate = DateOnly.FromDateTime(DateTime.Today);
                if (Order.DeliveryDate == default(DateOnly))
                    Order.DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
            }
        }

        private async Task OnSave()
        {
            // Диагностика
            System.Diagnostics.Debug.WriteLine($"=== OnSave START ===");
            System.Diagnostics.Debug.WriteLine($"CurrentUser: {CurrentUser?.UserId} - {CurrentUser?.Login}");
            System.Diagnostics.Debug.WriteLine($"IsEditMode: {IsEditMode}");
            System.Diagnostics.Debug.WriteLine($"Order: ID={Order.OrderId}, Date={Order.OrderDate}, Delivery={Order.DeliveryDate}, AddressId={Order.AddressId}, StatusId={Order.StatusId}");
            System.Diagnostics.Debug.WriteLine($"SelectedAddress: {SelectedAddress?.AddressId}, SelectedStatus: {SelectedStatus?.OrderStatusId}");

            // Валидация
            if (CurrentUser == null)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Пользователь не авторизован", "OK");
                return;
            }

            if (SelectedAddress == null)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Выберите адрес", "OK");
                return;
            }
            if (SelectedStatus == null)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Выберите статус", "OK");
                return;
            }
            if (Order.OrderDate == default(DateOnly) || Order.DeliveryDate == default(DateOnly))
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Даты не установлены", "OK");
                return;
            }
            if (Order.OrderDate > Order.DeliveryDate)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Дата доставки не может быть раньше даты заказа", "OK");
                return;
            }

            bool success;
            if (IsEditMode)
            {
                success = await _dbService.UpdateOrderAsync(Order);
            }
            else
            {
                if (CurrentUser.UserId <= 0)
                {
                    await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Некорректный ID пользователя", "OK");
                    return;
                }
                success = await _dbService.CreateOrderAsync(Order, CurrentUser.UserId);
            }

            System.Diagnostics.Debug.WriteLine($"Save result: {success}");

            if (success)
                await Shell.Current.GoToAsync("..");
            else
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Не удалось сохранить заказ", "OK");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}