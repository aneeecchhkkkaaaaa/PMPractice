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

        public async Task<List<Product>> GetProducts()
        {
            try
            { 
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
