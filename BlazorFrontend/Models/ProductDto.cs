namespace BlazorFrontend.Models;

/// <summary>
/// Mirrors ProductService.Api → ProductResponseDto.
/// Used when reading/displaying product data from the API.
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

/// <summary>
/// Mirrors ProductService.Api → CreateProductDto.
/// Used as the request body when creating a new product.
/// </summary>
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

/// <summary>
/// Mirrors ProductService.Api → UpdateProductDto.
/// Used as the request body when updating an existing product.
/// </summary>
public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
