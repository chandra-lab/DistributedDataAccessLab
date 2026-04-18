using BlazorFrontend.Models;
using System.Net.Http.Json;

namespace BlazorFrontend.Services;

public class ProductService
{
    private readonly HttpClient _http;

    public ProductService(HttpClient http) => _http = http;

    public async Task<List<Product>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<Product>>("api/products") ?? new();

    public async Task<bool> CreateAsync(CreateProductRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/products", request);
        return response.IsSuccessStatusCode;
    }
}
