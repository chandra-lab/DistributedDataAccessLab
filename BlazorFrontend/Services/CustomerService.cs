using BlazorFrontend.Models;
using System.Net.Http.Json;

namespace BlazorFrontend.Services;

public class CustomerService
{
    private readonly HttpClient _http;

    public CustomerService(HttpClient http) => _http = http;

    public async Task<List<Customer>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<Customer>>("api/customers") ?? new();

    public async Task<bool> CreateAsync(CreateCustomerRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/customers", request);
        return response.IsSuccessStatusCode;
    }
}
