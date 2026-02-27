using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Models;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly ICustomerClient _customerClient;

    public OrdersController(OrdersDbContext context, ICustomerClient customerClient)
    {
        _context = context;
        _customerClient = customerClient;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _context.Orders.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Order order)
    {
        var exists = await _customerClient.CustomerExistsAsync(order.CustomerId);
        if (!exists)
            return BadRequest("Customer does not exist.");

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        return Ok(order);
    }
}