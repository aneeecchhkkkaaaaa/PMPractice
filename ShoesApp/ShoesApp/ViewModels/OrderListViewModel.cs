using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ShoesApp.Infrastructures;
using ShoesApp.Models;

namespace ShoesApp.ViewModels
{
    [QueryProperty(nameof(CurrentUser), "CurrentUser")]
    public class OrderListViewModel : INotifyPropertyChanged
    {
        private readonly DbService _dbService = new();
        private User _currentUser;
        private ObservableCollection<Order> _orders = new();

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set { _orders = value; OnPropertyChanged(); }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanManage)); }
        }

        public bool CanManage => CurrentUser?.RoleId == 1;

        public ICommand LoadOrdersCommand { get; }
        public ICommand AddOrderCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }

        public OrderListViewModel()
        {
            LoadOrdersCommand = new Command(async () => await LoadOrders());
            AddOrderCommand = new Command(OnAddOrder);
            EditOrderCommand = new Command<Order>(OnEditOrder);
            DeleteOrderCommand = new Command<Order>(async (order) => await OnDeleteOrder(order));
        }

        private async Task LoadOrders()
        {
            var list = await _dbService.GetOrders();
            Orders.Clear();
            foreach (var order in list)
                Orders.Add(order);
        }

        private void OnAddOrder()
        {
            if (!CanManage) return;
            var param = new Dictionary<string, object>
            {
                { "CurrentUser", CurrentUser },
                { "IsEditMode", false }
            };
            Shell.Current.GoToAsync(nameof(OrderEditPage), param);
        }

        private void OnEditOrder(Order order)
        {
            if (!CanManage) return;
            var param = new Dictionary<string, object>
            {
                { "CurrentUser", CurrentUser },
                { "OrderToEdit", order },
                { "IsEditMode", true }
            };
            Shell.Current.GoToAsync(nameof(OrderEditPage), param);
        }

        private async Task OnDeleteOrder(Order order)
        {
            if (!CanManage) return;
            bool confirm = await Shell.Current.CurrentPage.DisplayAlert("Удаление", $"Удалить заказ №{order.OrderId}?", "Да", "Нет");
            if (confirm)
            {
                bool success = await _dbService.DeleteOrderAsync(order.OrderId);
                if (success) await LoadOrders();
                else await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Не удалось удалить заказ", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}