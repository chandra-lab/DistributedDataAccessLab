using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
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
        return Ok(orders);
    }

    // GET: api/orders/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    // POST: api/orders
    [HttpPost]
    public async Task<IActionResult> Create(Order order)
    {
        // Synchronous validation: check customer exists via HttpClient
        var customerExists = await _customerClient.CustomerExistsAsync(order.CustomerId);
        if (!customerExists)
            return BadRequest($"Customer with ID {order.CustomerId} does not exist.");

        // Synchronous validation: check product exists via HttpClient
        var productExists = await _productClient.ProductExistsAsync(order.ProductId);
        if (!productExists)
            return BadRequest($"Product with ID {order.ProductId} does not exist.");

        order.CreatedAt = DateTime.UtcNow;
        order.Status = "Pending";

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Publish event to RabbitMQ after saving
        _eventPublisher.PublishOrderCreated(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity
        });

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    // DELETE: api/orders/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
