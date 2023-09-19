using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1; // Replace with your actual project namespace
using Azure.Core;
using WebApplication1.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Tests
{
    

    [TestClass]
    public class ProductControllerIntegrationTests
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly string _jwtToken;
        private readonly ProductDbContext _context;
        private IServiceScope _scope;

        public ProductControllerIntegrationTests()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
            _jwtToken = GenerateJwtTokenAsync("your-client-id", "your-long-secret-key-256-bits").GetAwaiter().GetResult();
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        [TestInitialize]
        public void Initialize()
        {
            _context.Products.RemoveRange(_context.Products);
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task GetProductById_ReturnsProductIfExists()
        {
            var productToAdd = new Product
            {
                Name = "Test Product",
                Price = 999,
                Code = "Test Code",
                Description = "Description",
                Currency = "TRY",
                Brand = "Test Brand",
                Stock = 10
            };

            await _context.Products.AddAsync(productToAdd);
            await _context.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/Product/getById?id={productToAdd.Id}");

            response.EnsureSuccessStatusCode();
            var product = JsonConvert.DeserializeObject<Product>(await response.Content.ReadAsStringAsync());
           
            Assert.AreEqual(productToAdd.Id, product.Id);
            Assert.AreEqual(productToAdd.Name, product.Name);
            Assert.AreEqual(productToAdd.Price, product.Price);
            Assert.AreEqual(productToAdd.Code, product.Code);
            Assert.AreEqual(productToAdd.Brand, product.Brand);
            Assert.AreEqual(productToAdd.Stock, product.Stock);
            Assert.AreEqual(productToAdd.Currency, product.Currency);
            Assert.AreEqual(productToAdd.Description, product.Description);
        }

        [TestMethod]
        public async Task GetProductById_ReturnsNotFoundForNonexistentProduct()
        {
            var productToAdd = new Product
            {
                Name = "Test Product",
                Price = 999,
                Code = "Test Code",
                Description = "Description",
                Currency = "TRY",
                Brand = "Test Brand",
                Stock = 10
            };

            await _context.Products.AddAsync(productToAdd);
            await _context.SaveChangesAsync();

            var response = await _client.GetAsync("/api/Product/getById?id=9999"); 

            Assert.AreEqual(StatusCodes.Status404NotFound, (int)response.StatusCode);
        }

        [TestMethod]
        public async Task GetAllProducts_ReturnsSuccessStatusCode()
        {
            var response = await _client.GetAsync("/api/Product/getAll");

            response.EnsureSuccessStatusCode();
        }

        [TestMethod]
        public async Task SearchProducts_ReturnsMatchingProducts()
        {
            var productsToAdd = new List<Product>
            {
                new Product { Name = "Product A", Price = 100 , Code = "Test Code", Brand = "Test Brand", Currency = "USD", Description = "Test", Stock = 10},
                new Product { Name = "Product B", Price = 150 , Code = "Test Code", Brand = "Test Brand", Currency = "USD", Description = "Test", Stock = 10},
                new Product { Name = "Product C", Price = 200 , Code = "Test Code", Brand = "Test Brand", Currency = "USD", Description = "Test", Stock = 10},
            };
            await _context.Products.AddRangeAsync(productsToAdd);
            await _context.SaveChangesAsync();

            var searchCriteria = new ProductSearchCriteria
            {
                Code = "Test Code",
                Brand = "Test Brand",
                MinPrice = 90,
                MaxPrice = 150
            };
            var jsonRequest = JsonConvert.SerializeObject(searchCriteria);

            var response = await _client.PostAsync("/api/Product/search", new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            var products = JsonConvert.DeserializeObject<List<Product>>(await response.Content.ReadAsStringAsync());
            Assert.AreEqual(2, products.Count); 
        }

        [TestMethod]
        public async Task SearchProducts_ReturnsEmptyListForNoMatch()
        {
            var searchCriteria = new ProductSearchCriteria
            {
                Name = "Non-existent Product",
                MinPrice = 500,
                MaxPrice = 1000
            };
            var jsonRequest = JsonConvert.SerializeObject(searchCriteria);

            var response = await _client.PostAsync("/api/Product/search", new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            var products = JsonConvert.DeserializeObject<List<Product>>(await response.Content.ReadAsStringAsync());
            Assert.AreEqual(0, products.Count); 
        }

        [TestMethod]
        public async Task CreateProduct_ReturnsCreatedStatus()
        {
            var productToAdd = new Product
            {
                Name = "New Product",
                Price = 1999,
                Code = "New Code",
                Description = "New Description",
                Currency = "USD",
                Brand = "New Brand",
                Stock = 5
            };
            var jsonRequest = JsonConvert.SerializeObject(productToAdd);

            var response = await _client.PostAsync("/api/Product/create", new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

            Assert.AreEqual(StatusCodes.Status201Created, (int)response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateProduct_ReturnsNoContentForValidUpdate()
        {
            var productToUpdate = new Product
            {
                Name = "Existing Product",
                Price = 2999,
                Code = "Existing Code",
                Description = "Existing Description",
                Currency = "EUR",
                Brand = "Existing Brand",
                Stock = 10
            };
            await _context.Products.AddAsync(productToUpdate);
            await _context.SaveChangesAsync();

            productToUpdate.Name = "Updated Product";
            var jsonRequest = JsonConvert.SerializeObject(productToUpdate);

            var response = await _client.PutAsync($"/api/Product/update/{productToUpdate.Id}", new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

            Assert.AreEqual(StatusCodes.Status204NoContent, (int)response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateProduct_ReturnsBadRequestForInvalidUpdate()
        {
            // Arrange: Create a product in the database
            var productToUpdate = new Product
            {
                Name = "Existing Product",
                Price = 2999,
                Code = "Existing Code",
                Description = "Existing Description",
                Currency = "EUR",
                Brand = "Existing Brand",
                Stock = 10
            };
            await _context.Products.AddAsync(productToUpdate);
            await _context.SaveChangesAsync();

            productToUpdate.Name = null;
            var jsonRequest = JsonConvert.SerializeObject(productToUpdate);

            var response = await _client.PutAsync($"/api/Product/update/{productToUpdate.Id}", new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

            Assert.AreEqual(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteProduct_ReturnsNotFoundForNonexistentProduct()
        {
            var response = await _client.DeleteAsync("/api/Product/delete/9999"); 

            Assert.AreEqual(StatusCodes.Status404NotFound, (int)response.StatusCode);
        }

        private async Task<string> GenerateJwtTokenAsync(string clientId, string clientSecret)
        {
           
            var tokenRequest  = new TokenRequest { ClientId = clientId, ClientSecret = clientSecret };

            string jsonRequest = JsonConvert.SerializeObject(tokenRequest);

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage tokenResponse = await _client.PostAsync("/api/token", new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json"));

            return JsonConvert.DeserializeObject<dynamic>(await tokenResponse.Content.ReadAsStringAsync()).token;
        }
    }

}