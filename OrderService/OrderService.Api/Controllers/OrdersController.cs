using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.DTOs;
using OrderService.Api.Models;
using OrderService.Api.Services;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly ICustomerClient _customerClient;
    private readonly IProductClient _productClient;
    private readonly IEventPublisher _eventPublisher;

    public OrdersController(
        OrdersDbContext context,
        ICustomerClient customerClient,
        IProductClient productClient,
        IEventPublisher eventPublisher)
    {
        _context = context;
        _customerClient = customerClient;
        _productClient = productClient;
        _eventPublisher = eventPublisher;
    }

    // GET: api/orders
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _context.Orders.ToListAsync();
        var dtos = orders.Select(o => new OrderResponseDto
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            ProductId = o.ProductId,
            Quantity = o.Quantity,
            TotalPrice = o.TotalPrice,
            Status = o.Status,
            CreatedAt = o.CreatedAt
        });
        return Ok(dtos);
    }

    // GET: api/orders/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();
        return Ok(new OrderResponseDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity,
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        });
    }

    // POST: api/orders
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        var customerExists = await _customerClient.CustomerExistsAsync(dto.CustomerId);
        if (!customerExists)
            return BadRequest($"Customer with ID {dto.CustomerId} does not exist.");

        var productExists = await _productClient.ProductExistsAsync(dto.ProductId);
        if (!productExists)
            return BadRequest($"Product with ID {dto.ProductId} does not exist.");

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            TotalPrice = dto.TotalPrice,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Publish OrderCreated event
        _eventPublisher.PublishOrderCreated(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity
        });

        var responseDto = new OrderResponseDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity,
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, responseDto);
    }

    // DELETE: api/orders/5  (also publishes OrderCancelled event)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        // Publish OrderCancelled event
        _eventPublisher.PublishOrderCancelled(new OrderCancelledEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity
        });

        return NoContent();
    }
}
