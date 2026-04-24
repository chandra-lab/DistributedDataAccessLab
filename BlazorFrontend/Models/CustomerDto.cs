namespace BlazorFrontend.Models;

/// <summary>
/// Mirrors CustomerService.Api → CustomerResponseDto.
/// Used when reading/displaying customer data from the API.
/// </summary>
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

/// <summary>
/// Mirrors CustomerService.Api → CreateCustomerDto.
/// Used as the request body when creating a new customer.
/// </summary>
public class CreateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
