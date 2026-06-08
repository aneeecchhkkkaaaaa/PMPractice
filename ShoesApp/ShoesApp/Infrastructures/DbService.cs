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

        public async Task<User> GetUser(string login, string password)
        {
            var response = await _client.GetAsync(url + $"/api/Auth?login={login}&password={password}");
            return await response.Content.ReadFromJsonAsync<User>() ?? new();
        }

        public async Task<List<Product>> GetProducts()
        {
            try
            {
                // Стучимся на /api/Product (как в вашем контроллере) и ждем массив объектов в ответ
                var response = await _client.GetAsync(url + "/api/Product");

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

    }
}
