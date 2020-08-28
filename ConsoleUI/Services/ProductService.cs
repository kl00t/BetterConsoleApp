using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ConsoleUI.Interfaces;
using ConsoleUI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ConsoleUI.Services
{
    public class ProductService : IProductService
    {
        private readonly ILogger<GreetingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        public ProductService(ILogger<GreetingService> logger, IConfiguration configuration, HttpClient client)
        {
            _logger = logger;
            _configuration = configuration;
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:64195/")
            };
        }

        public async Task RunAsync()
        {
            Log.Logger.Information("Product Service Running");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                // Create a new product
                var product = new Product
                {
                    Name = "Gizmo",
                    Price = 100,
                    Category = "Widgets"
                };

                var url = await CreateProductAsync(product);
                Console.WriteLine($"Created at {url}");

                // Get the product
                product = await GetProductAsync(url.PathAndQuery);
                ShowProduct(product);

                // Update the product
                Console.WriteLine("Updating price...");
                product.Price = 80;
                await UpdateProductAsync(product);

                // Get the updated product
                product = await GetProductAsync(url.PathAndQuery);
                ShowProduct(product);

                // Delete the product
                var statusCode = await DeleteProductAsync(product.Id);
                Console.WriteLine($"Deleted (HTTP Status = {(int)statusCode})");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }

        void ShowProduct(Product product)
        {
            Console.WriteLine($"Name: {product.Name}\tPrice: " +
                              $"{product.Price}\tCategory: {product.Category}");
        }

        async Task<Uri> CreateProductAsync(Product product)
        {
            var response = await _client.PostAsJsonAsync(
                "api/products", product);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        async Task<Product> GetProductAsync(string path)
        {
            Product product = null;
            var response = await _client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                product = await response.Content.ReadAsAsync<Product>();
            }
            return product;
        }

        async Task<Product> UpdateProductAsync(Product product)
        {
            var response = await _client.PutAsJsonAsync($"api/products/{product.Id}", product);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            product = await response.Content.ReadAsAsync<Product>();
            return product;
        }

        async Task<HttpStatusCode> DeleteProductAsync(string id)
        {
            HttpResponseMessage response = await _client.DeleteAsync(
                $"api/products/{id}");
            return response.StatusCode;
        }
    }
}
