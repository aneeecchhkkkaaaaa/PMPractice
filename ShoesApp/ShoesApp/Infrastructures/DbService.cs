using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ShoesApp.Models;

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
    }
}
