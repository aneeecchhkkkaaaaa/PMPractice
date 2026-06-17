using ShoesApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShoesApp.Infrastructures
{
    class DbService
    {
        HttpClient _client = new HttpClient();

        string url = "http://localhost:5134";

        public async Task<User?> GetUser(string login, string password)
        {
            var payload = new { Login = login, Password = password };
            var response = await _client.PostAsJsonAsync(url + "/api/Auth", payload);
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadFromJsonAsync<User>() ?? new();
        }

        public async Task<List<Product>> GetProducts(string searchTerm, bool sortCount)
        {
            try
            {
                var response = await _client.GetAsync(url + $"/api/Products?sortCount={sortCount}&searchTerm={searchTerm}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Product>>() ?? new List<Product>();
                }

                return new List<Product>();
            }
            catch (Exception)
            {
                return new List<Product>();
            }
        }

        public async Task<List<Supplier>> GetSuppliers()
        {
            var response = await _client.GetAsync(url + "/api/Suppliers");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<Supplier>>() ?? new();
            return new();
        }

        public async Task<List<Product>> GetProducts(string searchTerm, bool sortCount, int? supplierId = null)
        {
            try
            {
                var urlParams = $"/api/Products?sortCount={sortCount}&searchTerm={Uri.EscapeDataString(searchTerm ?? "")}";
                if (supplierId.HasValue)
                    urlParams += $"&selectSupplierID={supplierId.Value}";

                var response = await _client.GetAsync(url + urlParams);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<Product>>() ?? new();
                return new();
            }
            catch
            {
                return new();
            }
        }
        public async Task<List<Manufacturer>> GetManufacturers() 
        {
            var response = await _client.GetAsync(url + "/api/Manufacturers");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<Manufacturer>>() ?? new();
            return new();
        }
        public async Task<bool> CreateProductAsync(Product product, FileResult? selectedImage) 
        {
            try
            {
                using var content = new MultipartFormDataContent();
                var productJson = JsonSerializer.Serialize(product, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
                content.Add(new StringContent(productJson, Encoding.UTF8, "application/json"), "product");

                if (selectedImage != null)
                {
                    var stream = await selectedImage.OpenReadAsync();
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(selectedImage.ContentType);
                    content.Add(streamContent, "image", selectedImage.FileName);
                }

                var response = await _client.PostAsync(url + "/api/Products", content);
                if (response.IsSuccessStatusCode)
                    return true;
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"CreateProduct error: {response.StatusCode} - {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateProduct exception: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> UpdateProductAsync(Product product, FileResult? selectedImage)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                var productJson = JsonSerializer.Serialize(product, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
                content.Add(new StringContent(productJson, Encoding.UTF8, "application/json"), "product");

                if (selectedImage != null)
                {
                    var stream = await selectedImage.OpenReadAsync();
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(selectedImage.ContentType);
                    content.Add(streamContent, "image", selectedImage.FileName);
                }

                var response = await _client.PutAsync($"{url}/api/Products/{product.ProductId}", content);
                if (response.IsSuccessStatusCode)
                    return true;
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"UpdateProduct error: {response.StatusCode} - {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateProduct exception: {ex.Message}");
                return false;
            }
        }
        public async Task<(bool success, string error)> DeleteProductAsync(string productId) 
        {
            try
            {
                var response = await _client.DeleteAsync($"{url}/api/Products/{productId}");
                if (response.IsSuccessStatusCode)
                    return (true, string.Empty);
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return (false, error);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<List<Order>> GetOrders()
        {
            var response = await _client.GetAsync(url + "/api/Orders");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<Order>>() ?? new();
            return new();
        }

        public async Task<List<Address>> GetAddresses()
        {
            var response = await _client.GetAsync(url + "/api/Addresses");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<Address>>() ?? new();
            return new();
        }

        public async Task<List<OrderStatus>> GetOrderStatuses()
        {
            var response = await _client.GetAsync(url + "/api/OrderStatuses");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<OrderStatus>>() ?? new();
            return new();
        }

        public async Task<bool> CreateOrderAsync(Order order, int userId)
        {
            var dto = new OrderCreateDto
            {
                OrderDate = order.OrderDate,
                DeliveryDate = order.DeliveryDate,
                AddressId = order.AddressId,
                StatusId = order.StatusId,
                UserId = userId
            };
            var response = await _client.PostAsJsonAsync(url + "/api/Orders", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            var dto = new OrderUpdateDto
            {
                OrderDate = order.OrderDate,
                DeliveryDate = order.DeliveryDate,
                AddressId = order.AddressId,
                StatusId = order.StatusId
            };
            var response = await _client.PutAsJsonAsync($"{url}/api/Orders/{order.OrderId}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var response = await _client.DeleteAsync($"{url}/api/Orders/{orderId}");
            return response.IsSuccessStatusCode;
        }
        public class OrderCreateDto
        {
            public DateOnly OrderDate { get; set; }
            public DateOnly DeliveryDate { get; set; }
            public int AddressId { get; set; }
            public int StatusId { get; set; }
            public int UserId { get; set; }
        }

        public class OrderUpdateDto
        {
            public DateOnly OrderDate { get; set; }
            public DateOnly DeliveryDate { get; set; }
            public int AddressId { get; set; }
            public int StatusId { get; set; }
        }
    }
}
