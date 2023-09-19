using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using product_client;

class Program
{
    static async Task Main(string[] args)
    {
        string apiUrl = "https://localhost:7265";
        string tokenEndpoint = apiUrl + "/api/token"; 

        string clientId = "your-client-id";
        string clientSecret = "your-long-secret-key-256-bits";

        var tokenRequest = new
        {
            ClientId = clientId,
            ClientSecret = clientSecret
        };

        string jsonRequest = JsonConvert.SerializeObject(tokenRequest);

        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            
            HttpResponseMessage tokenResponse = await httpClient.PostAsync(tokenEndpoint, new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json"));

            if (tokenResponse.IsSuccessStatusCode)
            {
                
                string jsonResponse = await tokenResponse.Content.ReadAsStringAsync();
                var tokenData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                string accessToken = tokenData.token;
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var endpoints = new Dictionary<int, string>
                {
                    { 1, "/api/Product/getAll" },
                    { 2, "/api/Product/getById" },
                    { 3, "/api/Product/create" },
                    { 4, "/api/Product/update/{id}" },
                    { 5, "/api/Product/delete/{id}" },
                    { 6, "/api/Product/search" },
                };

                bool exit = false;

                while (!exit)
                {
                    Console.WriteLine("Choose an endpoint:");
                    foreach (var endpoint in endpoints)
                    {
                        Console.WriteLine($"{endpoint.Key}. {endpoint.Value}");
                    }
                    Console.WriteLine("0. Quit");

                    
                    Console.Write("Enter the number of the endpoint (or 0 to quit): ");
                    if (int.TryParse(Console.ReadLine(), out int selectedEndpoint))
                    {
                        if (selectedEndpoint == 0)
                        {
                            exit = true; 
                        }
                        else if (endpoints.ContainsKey(selectedEndpoint))
                        {
                            string selectedEndpointUrl = endpoints[selectedEndpoint];


                            switch (selectedEndpoint)
                            {
                                case 1:
                                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl + selectedEndpointUrl);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        var products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();

                                        Console.WriteLine("List of all products: ");
                                        foreach (var product in products)
                                        {
                                            Console.WriteLine($"ID: {product.Id}, Name: {product.Name}, Description: {product.Description}, Code: {product.Code}, Brand: {product.Brand}, Price: {product.Price}, Currency: {product.Currency}, Stock: {product.Stock}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Error: {response.StatusCode}");
                                    }
                                    break;
                                case 2:
                                    Console.Write("Enter Id of the product: ");
                                    var id = Console.ReadLine();

                                    // Construct the URL with the id parameter
                                    var requestUri = new Uri(apiUrl + selectedEndpointUrl + "?id=" + id);

                                    HttpResponseMessage responsev2 = await httpClient.GetAsync(requestUri);

                                    if (responsev2.IsSuccessStatusCode)
                                    {
                                        var product = await responsev2.Content.ReadFromJsonAsync<ProductDto>();
                                        Console.WriteLine($"ID: {product.Id}, Name: {product.Name}, Description: {product.Description}, Code: {product.Code}, Brand: {product.Brand}, Price: {product.Price}, Currency: {product.Currency}, Stock: {product.Stock}");
                                    }
                                    else
                                    { 
                                        Console.WriteLine($"Error: {responsev2.StatusCode}");
                                    }
                                    break;
                                case 3:
                                    Console.WriteLine("Enter product properties to be created: ");
                                    Console.Write("Name: ");
                                    var name = Console.ReadLine();
                                    Console.Write("Code: ");
                                    var code = Console.ReadLine();
                                    Console.Write("Brand: ");
                                    var brand = Console.ReadLine();
                                    Console.Write("Price: ");
                                    var priceInput = Console.ReadLine();
                                    decimal price;
                                    if (!decimal.TryParse(priceInput, out price))
                                    {
                                        Console.WriteLine("Invalid price format. Please enter a valid decimal number.");
                                        break;
                                    }
                                    Console.Write("Description: ");
                                    var description = Console.ReadLine();
                                    Console.Write("Currency: ");
                                    var currency = Console.ReadLine();
                                    Console.Write("Stock: ");
                                    var stockInput = Console.ReadLine();
                                    int stock;
                                    if (!int.TryParse(stockInput, out stock))
                                    {
                                        Console.WriteLine("Invalid stock format. Please enter a valid integer.");
                                        break;
                                    }

                                    var productv2 = new ProductDto {
                                        Name = name,
                                        Code = code,
                                        Brand = brand,
                                        Price = price,
                                        Description = description,
                                        Currency = currency,
                                        Stock = stock
                                    };

                                    HttpResponseMessage responsev3 = await httpClient.PostAsJsonAsync(apiUrl + selectedEndpointUrl, productv2);

                                    if (responsev3.IsSuccessStatusCode)
                                    {
                                        Console.WriteLine("Product successfully created");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Error: {responsev3.StatusCode}");
                                    }
                                    break;
                                case 4:
                                    Console.Write("Enter the ID of the product to update: ");
                                    var idInput = Console.ReadLine();
                                    if (!int.TryParse(idInput, out int updateId))
                                    {
                                        Console.WriteLine("Invalid ID format. Please enter a valid integer.");
                                        break;
                                    }

                                    Console.WriteLine("Enter updated product properties: ");
                                    Console.Write("Name: ");
                                    var updateName = Console.ReadLine();
                                    Console.Write("Code: ");
                                    var updateCode = Console.ReadLine();
                                    Console.Write("Brand: ");
                                    var updateBrand = Console.ReadLine();
                                    Console.Write("Price: ");
                                    var updatePriceInput = Console.ReadLine();
                                    decimal updatePrice;
                                    if (!decimal.TryParse(updatePriceInput, out updatePrice))
                                    {
                                        Console.WriteLine("Invalid price format. Please enter a valid decimal number.");
                                        break;
                                    }
                                    Console.Write("Description: ");
                                    var updateDescription = Console.ReadLine();
                                    Console.Write("Currency: ");
                                    var updateCurrency = Console.ReadLine();
                                    Console.Write("Stock: ");
                                    var updateStockInput = Console.ReadLine();
                                    int updateStock;
                                    if (!int.TryParse(updateStockInput, out updateStock))
                                    {
                                        Console.WriteLine("Invalid stock format. Please enter a valid integer.");
                                        break;
                                    }

                                    var updatedProduct = new ProductDto
                                    {
                                        Id = updateId,
                                        Name = updateName,
                                        Code = updateCode,
                                        Brand = updateBrand,
                                        Price = updatePrice,
                                        Description = updateDescription,
                                        Currency = updateCurrency,
                                        Stock = updateStock
                                    };

                                    HttpResponseMessage responsev4 = await httpClient.PutAsJsonAsync(apiUrl + $"/api/Product/update/{updateId}", updatedProduct);

                                    if (responsev4.IsSuccessStatusCode)
                                    {
                                        Console.WriteLine("Product updated successfully.");
                                    }
                                    else if (responsev4.StatusCode == HttpStatusCode.BadRequest)
                                    {
                                        Console.WriteLine("Bad request. Please check your input data.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("An error occurred while updating the product.");
                                    }
                                    break;

                                case 5:
                                    Console.Write("Enter the ID of the product to delete: ");
                                    var deleteIdInput = Console.ReadLine();
                                    if (!int.TryParse(deleteIdInput, out int deleteId))
                                    {
                                        Console.WriteLine("Invalid ID format. Please enter a valid integer.");
                                        break;
                                    }

                                    HttpResponseMessage responsev5 = await httpClient.DeleteAsync(apiUrl + $"/api/Product/delete/{deleteId}");

                                    if (responsev5.IsSuccessStatusCode)
                                    {
                                        Console.WriteLine("Product deleted successfully.");
                                    }
                                    else if (responsev5.StatusCode == HttpStatusCode.NotFound)
                                    {
                                        Console.WriteLine("Product not found. Please check the ID.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("An error occurred while deleting the product.");
                                    }
                                    break;

                                case 6:
                                    Console.Write("Enter product name (leave empty to skip): ");
                                    var searchName = Console.ReadLine();
                                    Console.Write("Enter product code (leave empty to skip): ");
                                    var searchCode = Console.ReadLine();
                                    Console.Write("Enter product brand (leave empty to skip): ");
                                    var searchBrand = Console.ReadLine();
                                    Console.Write("Enter minimum price (leave empty to skip): ");
                                    var minPriceInput = Console.ReadLine();
                                    decimal? minPrice = string.IsNullOrWhiteSpace(minPriceInput) ? null : (decimal?)decimal.Parse(minPriceInput);
                                    Console.Write("Enter maximum price (leave empty to skip): ");
                                    var maxPriceInput = Console.ReadLine();
                                    decimal? maxPrice = string.IsNullOrWhiteSpace(maxPriceInput) ? null : (decimal?)decimal.Parse(maxPriceInput);

                                    var searchCriteria = new ProductSearchCriteriaDto
                                    {
                                        Name = searchName,
                                        Code = searchCode,
                                        Brand = searchBrand,
                                        MinPrice = minPrice,
                                        MaxPrice = maxPrice
                                    };

                                    var jsonContent = JsonConvert.SerializeObject(searchCriteria);

                                    var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                                    HttpResponseMessage responsev6 = await httpClient.PostAsync(apiUrl + selectedEndpointUrl, httpContent);

                                    if (responsev6.IsSuccessStatusCode)
                                    {
                                        var responseBody = await responsev6.Content.ReadAsStringAsync();
                                        var searchProducts = JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(responseBody);
                                        Console.WriteLine("Products found:");
                                        foreach (var product in searchProducts)
                                        {
                                            Console.WriteLine($"ID: {product.Id}, Name: {product.Name}, Description: {product.Description}, Code: {product.Code}, Brand: {product.Brand}, Price: {product.Price}, Currency: {product.Currency}, Stock: {product.Stock}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("An error occurred while searching for products.");
                                    }
                                    break;
                            }

                        }
                        else
                        {
                            Console.WriteLine("Invalid selection.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a number.");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Token Error: {tokenResponse.StatusCode}");
            }
        }
    }
}
