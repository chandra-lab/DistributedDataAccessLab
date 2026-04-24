using BlazorFrontend.Models;
using System.Net.Http.Json;

namespace BlazorFrontend.Services;

public class OrderService
{
    private readonly HttpClient _http;

    public OrderService(HttpClient http) => _http = http;

    public async Task<List<OrderDto>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<OrderDto>>("api/orders") ?? new();

    public async Task<bool> CreateAsync(CreateOrderDto request)
    {
        var response = await _http.PostAsJsonAsync("api/orders", request);
        return response.IsSuccessStatusCode;
    }
}
