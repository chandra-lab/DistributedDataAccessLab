using BlazorFrontend.Models;
using System.Net.Http.Json;

namespace BlazorFrontend.Services;

public class ProductService
{
    private readonly HttpClient _http;

    public ProductService(HttpClient http) => _http = http;

    public async Task<List<ProductDto>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<ProductDto>>("api/products") ?? new();

    public async Task<bool> CreateAsync(CreateProductDto request)
    {
        var response = await _http.PostAsJsonAsync("api/products", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto request)
    {
        var response = await _http.PutAsJsonAsync($"api/products/{id}", request);
        return response.IsSuccessStatusCode;
    }
}
