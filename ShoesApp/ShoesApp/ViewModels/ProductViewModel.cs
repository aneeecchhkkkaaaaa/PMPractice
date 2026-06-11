using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ShoesApp.Infrastructures;
using ShoesApp.Models;

namespace ShoesApp.ViewModels
{
    [QueryProperty(nameof(ReceivedUser), "CurrentUser")]
    internal class ProductViewModel : INotifyPropertyChanged
    {
        // Инициализация полей
        private readonly DbService _dbService = new DbService();
        private User? _receivedUser;
        private string _userFIO = "Гость";
        private string _searchTerm = string.Empty;
        private bool _isSortAsc;
        private bool _isSortDesc;
        //private string _userRole = "Гость";  // На возможный функционал
        private bool _isMeneger = false;
        private bool _isAdministrator = false;

        public bool IsSortAsc
        {
            get => _isSortAsc;
            set
            {
                if (_isSortAsc != value)
                {
                    _isSortAsc = value;
                    if (value)
                    {
                        _isSortDesc = false;
                        OnPropertyChanged(nameof(IsSortDesc));
                    }
                    OnPropertyChanged();
                    LoadProductsAsync();
                }
            }
        }

        public bool SearchVisible
        {
            get => (_isMeneger || _isAdministrator);
        }

        public bool IsSortDesc
        {
            get => _isSortDesc;
            set
            {
                if (_isSortDesc != value)
                {
                    _isSortDesc = value;
                    if (value)
                    {
                        _isSortAsc = false;
                        OnPropertyChanged(nameof(IsSortAsc));
                    }
                    OnPropertyChanged();
                    LoadProductsAsync(); 
                }
            }
        }

        public string SearchTerm 
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
            }
        }
        
        // Установка данных пользователя
        public User ReceivedUser
        {
            get => _receivedUser;
            set
            {
                _receivedUser = value;
                UserFIO = $"{_receivedUser?.LastName} {_receivedUser?.FirstName} {_receivedUser?.Patronymic}" ?? "Гость";
                //_userRole = _receivedUser.Role?.RolesName; // На возможный функционал
                if (_receivedUser != null)
                {
                    switch (_receivedUser.RoleId)
                    {
                        case 1: _isAdministrator = true; _isMeneger = false; break;
                        case 2: _isMeneger = true; _isAdministrator = false; break;
                        default: _isAdministrator = false; _isMeneger = false; break;
                    }
                    OnPropertyChanged();
                }
                OnPropertyChanged(nameof(SearchVisible));
            }
        }

        

        // Привязка для имени пользователя в TitleView
        public string UserFIO
        {
            get => _userFIO;
            set { _userFIO = value; OnPropertyChanged(); }
        }


        // Коллекция оберток для CollectionView (верстка будет брать данные отсюда)
        public ObservableCollection<ProductItemViewModel> Products { get; set; } = new ObservableCollection<ProductItemViewModel>();

        // Команды
        public ICommand LogoutCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public ProductViewModel()
        {
            LogoutCommand = new Command(OnLogout);
            DeleteProductCommand = new Command<ProductItemViewModel>(async (item) => await OnDeleteProduct(item));

            // Загрузка данных при старте
            _ = LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                Products.Clear();

                bool sortAscending = IsSortAsc;

                if (IsSortDesc)
                    sortAscending = false;

                List<Product> originalProducts = await _dbService.GetProducts(_searchTerm, sortAscending);

                foreach (var product in originalProducts)
                {
                    Products.Add(new ProductItemViewModel(product));
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
        }

        private async Task OnDeleteProduct(ProductItemViewModel item)
        {
            if (item == null) return;

            bool confirm = await Shell.Current.CurrentPage.DisplayAlert(
                "Удаление",
                $"Вы уверены, что хотите удалить {item.Title}?",
                "Да", "Нет");

            if (confirm)
            {
                try
                {
                    // Вызов удаления из БД (передаем оригинальный ProductId или объект)
                    // await _dbService.DeleteProduct(item.OriginalProduct.ProductId);

                    Products.Remove(item);
                }
                catch (Exception ex)
                {
                    await Shell.Current.CurrentPage.DisplayAlert("Ошибка", $"Не удалось удалить: {ex.Message}", "OK");
                }
            }
        }

        private async void OnLogout()
        {
            ReceivedUser = null;
            await Shell.Current.GoToAsync("..");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    internal class ProductItemViewModel
    {
        public Product OriginalProduct { get; }

        // Маппинг базовых свойств под имена из XAML
        public string Title => OriginalProduct.ProductName;
        public string Description => OriginalProduct.Description;
        public decimal Price => OriginalProduct.Price;
        public int StockQuantity => OriginalProduct.InWarehouse;
        public string Unit => OriginalProduct.UnitOfMeasurement;

        public ImageSource Photo
        {
            get
            {
                if (string.IsNullOrEmpty(OriginalProduct.Photo))
                    return ImageSource.FromFile("picture.png");

                string fullUrl = OriginalProduct.Photo;
                try
                {
                    return ImageSource.FromUri(new Uri(fullUrl));
                }
                catch
                {
                    return ImageSource.FromFile("picture.png");
                }
            }
        }

        // Маппинг связанных сущностей
        public CategoryWrapper Category => new CategoryWrapper { CategoryName = OriginalProduct.Category };
        public Manufacturer Manufacturer => OriginalProduct.Manufacturer;
        public SupplierWrapper Supplier => new SupplierWrapper { Supliername = OriginalProduct.Supplier?.SupplierName ?? "Не указан" };
        public int Discount => OriginalProduct.Current;
        public bool HasDiscount => Discount > 0;
        public bool HasNoDiscount => Discount == 0;
        public decimal PriceWithDiscount => HasDiscount ? Price * (1 - (decimal)Discount / 100) : Price;

        // Свойства для триггеров (DataTrigger) в XAML
        public bool IsOutOfStock => StockQuantity == 0;
        public bool IsHighDiscount => Discount >= 15;

        public ProductItemViewModel(Product product)
        {
            OriginalProduct = product;
        }
    }

    // Вспомогательные заглушки-обертки для совпадения с путями в XAML (Category.CategoryName и Supplier.Supliername)
    internal class CategoryWrapper { public string CategoryName { get; set; } }
    internal class SupplierWrapper { public string Supliername { get; set; } }
}