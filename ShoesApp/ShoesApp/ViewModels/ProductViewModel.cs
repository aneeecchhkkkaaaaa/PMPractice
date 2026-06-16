using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ShoesApp.Infrastructures;
using ShoesApp.Models;

namespace ShoesApp.ViewModels
{
    [QueryProperty(nameof(ReceivedUser), "CurrentUser")]
    internal class ProductViewModel : INotifyPropertyChanged
    {
        private readonly DbService _dbService = new();
        private User? _receivedUser;
        private string _userFIO = "Гость";
        private string _searchTerm = string.Empty;
        private bool _isSortAsc;
        private bool _isSortDesc;
        private bool _isMeneger = false;
        private bool _isAdministrator = false;
        private int? _selectedSupplierId;
        private ObservableCollection<Supplier> _suppliers = new();
        private CancellationTokenSource _searchCts;
        private Supplier _selectedSupplier;

        public ProductViewModel()
        {
            LogoutCommand = new Command(OnLogout);
            AddProductCommand = new Command(OnAddProduct);
            EditProductCommand = new Command<Product>(OnEditProduct);
            DeleteProductCommand = new Command<Product>(async (product) => await OnDeleteProduct(product));

            _ = LoadProductsAsync();
            LoadSuppliersAsync();
        }

        // Свойства для привязки
        public Supplier SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                if (_selectedSupplier != value)
                {
                    _selectedSupplier = value;
                    OnPropertyChanged();
                    int? supplierForApi = (value?.SupplierId == 0) ? null : value?.SupplierId;
                    SelectedSupplierId = supplierForApi;
                    _ = LoadProductsAsync();
                }
            }
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get => _suppliers;
            set { _suppliers = value; OnPropertyChanged(); }
        }

        public int? SelectedSupplierId
        {
            get => _selectedSupplierId;
            set
            {
                if (_selectedSupplierId != value)
                {
                    _selectedSupplierId = value;
                    OnPropertyChanged();
                    //_ = LoadProductsAsync();
                }
            }
        }

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
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    OnPropertyChanged();
                    _searchCts?.Cancel();
                    _searchCts = new CancellationTokenSource();
                    Task.Delay(500, _searchCts.Token).ContinueWith(_ =>
                    {
                        if (!_.IsCanceled)
                            MainThread.BeginInvokeOnMainThread(() => LoadProductsAsync());
                    }, TaskScheduler.Default);
                }
            }
        }

        public User ReceivedUser
        {
            get => _receivedUser;
            set
            {
                _receivedUser = value;
                UserFIO = $"{_receivedUser?.LastName} {_receivedUser?.FirstName} {_receivedUser?.Patronymic}" ?? "Гость";
                if (_receivedUser != null)
                {
                    switch (_receivedUser.RoleId)
                    {
                        case 1: _isAdministrator = true; _isMeneger = false; break;
                        case 2: _isMeneger = true; _isAdministrator = false; break;
                        default: _isAdministrator = false; _isMeneger = false; break;
                    }
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsAdministrator));
                }
                OnPropertyChanged(nameof(SearchVisible));
            }
        }

        public string UserFIO
        {
            get => _userFIO;
            set { _userFIO = value; OnPropertyChanged(); }
        }

        public bool SearchVisible => (_isMeneger || _isAdministrator);
        public bool IsAdministrator => _isAdministrator;

        public ObservableCollection<ProductItemViewModel> Products { get; set; } = new();

        // Команды
        public ICommand LogoutCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        private async Task LoadSuppliersAsync()
        {
            var list = await _dbService.GetSuppliers();
            Suppliers.Clear();
            Suppliers.Add(new Supplier { SupplierId = 0, SupplierName = "Все поставщики" });
            foreach (var s in list)
                Suppliers.Add(s);
            SelectedSupplier = Suppliers.First();
        }

        public async Task LoadProductsAsync()
        {
            try
            {
                Products.Clear();
                bool sortAscending = IsSortAsc;
                if (IsSortDesc) sortAscending = false;

                int? supplierForApi = (SelectedSupplierId == 0) ? null : SelectedSupplierId;
                var originalProducts = await _dbService.GetProducts(_searchTerm, sortAscending, supplierForApi);
                foreach (var product in originalProducts)
                    Products.Add(new ProductItemViewModel(product));
            }
            catch (Exception ex)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
        }

        private void OnAddProduct()
        {
            if (IsAdministrator)
                Shell.Current.GoToAsync(nameof(ProductEditPage));
        }

        private void OnEditProduct(Product product)
        {
            if (product == null || !IsAdministrator) return;
            var navigationParam = new Dictionary<string, object>
            {
                { "SelectedProduct", JsonSerializer.Serialize(product) }
            };
            Shell.Current.GoToAsync(nameof(ProductEditPage), navigationParam);
        }

        private async Task OnDeleteProduct(Product product)
        {
            if (product == null || !IsAdministrator) return;

            bool confirm = await Shell.Current.CurrentPage.DisplayAlert("Удаление",
                $"Вы уверены, что хотите удалить {product.ProductName}?", "Да", "Нет");
            if (confirm)
            {
                var (success, error) = await _dbService.DeleteProductAsync(product.ProductId);
                if (success)
                {
                    var item = Products.FirstOrDefault(p => p.OriginalProduct.ProductId == product.ProductId);
                    if (item != null)
                        Products.Remove(item);
                }
                else
                {
                    await Shell.Current.CurrentPage.DisplayAlert("Ошибка", error, "OK");
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

        public CategoryWrapper Category => new CategoryWrapper { CategoryName = OriginalProduct.Category };
        public Manufacturer Manufacturer => OriginalProduct.Manufacturer;
        public SupplierWrapper Supplier => new SupplierWrapper { Supliername = OriginalProduct.Supplier?.SupplierName ?? "Не указан" };
        public int Discount => OriginalProduct.Current;
        public bool HasDiscount => Discount > 0;
        public bool HasNoDiscount => Discount == 0;
        public decimal PriceWithDiscount => HasDiscount ? Price * (1 - (decimal)Discount / 100) : Price;
        public bool IsOutOfStock => StockQuantity == 0;
        public bool IsHighDiscount => Discount >= 15;

        public ProductItemViewModel(Product product)
        {
            OriginalProduct = product;
        }
    }

    internal class CategoryWrapper { public string CategoryName { get; set; } }
    internal class SupplierWrapper { public string Supliername { get; set; } }
}