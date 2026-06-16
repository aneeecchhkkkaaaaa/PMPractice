using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using ShoesApp.Infrastructures;
using ShoesApp.Models;

namespace ShoesApp.ViewModels
{
    [QueryProperty(nameof(SelectedProductJson), "SelectedProduct")]
    public class ProductEditViewModel : INotifyPropertyChanged
    {
        private readonly DbService _dbService = new();
        private Product _product = new();
        private FileResult? _selectedImage;
        private List<Supplier> _suppliers = new();
        private List<Manufacturer> _manufacturers = new();
        private bool _isEditMode;
        private string _title = "Добавление товара";
        private Manufacturer? _selectedManufacturer;
        private Supplier? _selectedSupplier;

        public List<string> Units { get; } = new() { "шт", "пара", "кг", "м", "уп" };
        public List<string> Categories { get; } = new() { "Кроссовки", "Ботинки", "Сандалии", "Туфли", "Сапоги" };

        public Product Product
        {
            get => _product;
            set { _product = value; OnPropertyChanged(); }
        }

        public FileResult? SelectedImage
        {
            get => _selectedImage;
            set { _selectedImage = value; OnPropertyChanged(); OnPropertyChanged(nameof(ImagePreview)); }
        }

        public string? ImagePreview
        {
            get
            {
                if (SelectedImage != null)
                    return SelectedImage.FullPath;
                if (IsEditMode && !string.IsNullOrEmpty(Product.Photo))
                    return $"http://localhost:5134/images/{Product.Photo}";
                return null;
            }
        }

        public List<Supplier> Suppliers
        {
            get => _suppliers;
            set { _suppliers = value; OnPropertyChanged(); }
        }

        public List<Manufacturer> Manufacturers
        {
            get => _manufacturers;
            set { _manufacturers = value; OnPropertyChanged(); }
        }

        public Manufacturer? SelectedManufacturer
        {
            get => _selectedManufacturer;
            set
            {
                if (_selectedManufacturer != value)
                {
                    _selectedManufacturer = value;
                    if (value != null)
                        Product.ManufacturerId = value.ManufacturerId;
                    OnPropertyChanged();
                }
            }
        }

        public Supplier? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                if (_selectedSupplier != value)
                {
                    _selectedSupplier = value;
                    if (value != null)
                        Product.SupplierId = value.SupplierId;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(Title)); }
        }

        public string Title => IsEditMode ? "Редактирование товара" : "Добавление товара";

        public ICommand SaveCommand { get; }
        public ICommand PickImageCommand { get; }

        public string SelectedProductJson
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var prod = JsonSerializer.Deserialize<Product>(value);
                    if (prod != null)
                    {
                        Product = prod;
                        IsEditMode = true;
                        LoadSelectedItemsWhenReady();
                    }
                }
            }
        }

        public ProductEditViewModel()
        {
            SaveCommand = new Command(async () => await OnSave());
            PickImageCommand = new Command(async () => await OnPickImage());
            LoadSuppliersAndManufacturers();
        }

        private async void LoadSuppliersAndManufacturers()
        {
            Suppliers = await _dbService.GetSuppliers();
            Manufacturers = await _dbService.GetManufacturers();
            if (IsEditMode)
                LoadSelectedItems();
        }

        private async void LoadSelectedItemsWhenReady()
        {
            while (Manufacturers.Count == 0 || Suppliers.Count == 0)
                await Task.Delay(100);
            LoadSelectedItems();
        }

        private void LoadSelectedItems()
        {
            SelectedManufacturer = Manufacturers.FirstOrDefault(m => m.ManufacturerId == Product.ManufacturerId);
            SelectedSupplier = Suppliers.FirstOrDefault(s => s.SupplierId == Product.SupplierId);
        }

        private async Task OnPickImage()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Выберите изображение",
                    FileTypes = FilePickerFileType.Images
                });
                if (result != null)
                    SelectedImage = result;
            }
            catch (Exception ex)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private async Task OnSave()
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(Product.ProductName))
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Введите наименование товара", "OK");
                return;
            }
            if (Product.Price < 0)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Цена не может быть отрицательной", "OK");
                return;
            }
            if (Product.InWarehouse < 0)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Количество на складе не может быть отрицательным", "OK");
                return;
            }
            if (Product.Current < 0 || Product.Current > 100)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Скидка должна быть от 0 до 100", "OK");
                return;
            }
            if (Product.SupplierId == 0)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Выберите поставщика", "OK");
                return;
            }
            if (Product.ManufacturerId == 0)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Выберите производителя", "OK");
                return;
            }

            // Создаём копию без навигационных свойств для отправки
            var productToSend = new Product
            {
                ProductId = Product.ProductId,
                ProductName = Product.ProductName,
                UnitOfMeasurement = Product.UnitOfMeasurement,
                Price = Product.Price,
                SupplierId = Product.SupplierId,
                ManufacturerId = Product.ManufacturerId,
                Category = Product.Category,
                Current = Product.Current,
                InWarehouse = Product.InWarehouse,
                Description = Product.Description,
            };

            bool success;
            if (IsEditMode)
                success = await _dbService.UpdateProductAsync(productToSend, SelectedImage);
            else
                success = await _dbService.CreateProductAsync(productToSend, SelectedImage);

            if (success)
                await Shell.Current.GoToAsync("..");
            else
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Не удалось сохранить товар", "OK");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}