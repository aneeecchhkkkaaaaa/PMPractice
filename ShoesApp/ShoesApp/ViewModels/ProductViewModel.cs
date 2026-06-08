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
        private readonly DbService _dbService = new DbService();
        private User _receivedUser;
        private string _userName = "Гость";


        public User ReceivedUser
        {
            get => _receivedUser;
            set
            {
                _receivedUser = value;
                UserName = _receivedUser?.FirstName ?? "Гость";
                OnPropertyChanged();
            }
        }

        // Привязка для имени пользователя в TitleView
        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); }
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

            // Запуск загрузки данных при старте
            _ = LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                Products.Clear();

                // Вызываем обновленный метод, который теперь отдает List<Product>
                List<Product> originalProducts = await _dbService.GetProducts();

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
    

    /// <summary>
    /// Класс-адаптер. Связывает оригинальный Product со свойствами из вашей XAML-верстки
    /// </summary>
    internal class ProductItemViewModel
    {
        public Product OriginalProduct { get; }

        // Маппинг базовых свойств под имена из XAML
        public string Title => OriginalProduct.ProductName;
        public string Description => OriginalProduct.Description;
        public ImageSource PhotoUrl => ImageSource.FromUri(new Uri($"https://localhost:7053/Images/{OriginalProduct.Photo}"));
        public decimal Price => OriginalProduct.Price;
        public int StockQuantity => OriginalProduct.InWarehouse;
        public string Unit => OriginalProduct.UnitOfMeasurement;

        public ImageSource Photo
        {
            get
            {
                if (string.IsNullOrEmpty(OriginalProduct.Photo))
                    return ImageSource.FromFile("picture.png");
                string baseUrl = "https://localhost:7053";
                string fullUrl = $"{baseUrl}/Images/{OriginalProduct.Photo}";
                return ImageSource.FromUri(new Uri(fullUrl));
            }
        }
        // Маппинг связанных сущностей
        public CategoryWrapper Category => new CategoryWrapper { CategoryName = OriginalProduct.Category };
        public Manufacturer Manufacturer => OriginalProduct.Manufacturer;
        public SupplierWrapper Supplier => new SupplierWrapper { Supliername = OriginalProduct.Supplier?.SupplierName ?? "Не указан" };

        // Логика скидок (Вы можете заменить эти формулы на ваши реальные данные из БД)
        public int Discount => OriginalProduct.Current; // Предполагаем, что текущая скидка хранится в Current
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
