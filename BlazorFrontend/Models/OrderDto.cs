namespace BlazorFrontend.Models;

/// <summary>
/// Mirrors OrderService.Api → OrderResponseDto.
/// Used when reading/displaying order data from the API.
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Mirrors OrderService.Api → CreateOrderDto.
/// Used as the request body when placing a new order.
/// </summary>
public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
