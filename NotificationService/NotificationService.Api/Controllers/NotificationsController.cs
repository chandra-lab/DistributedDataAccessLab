using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Data;
using NotificationService.Api.DTOs;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationDbContext _context;

    public NotificationsController(NotificationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var notifications = await _context.Notifications.ToListAsync();
        var dtos = notifications.Select(n => new NotificationResponseDto
        {
            Id = n.Id,
            OrderId = n.OrderId,
            CustomerId = n.CustomerId,
            Message = n.Message,
            EventType = n.EventType,
            CreatedAt = n.CreatedAt
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var n = await _context.Notifications.FindAsync(id);
        if (n == null) return NotFound();
        return Ok(new NotificationResponseDto
        {
            Id = n.Id,
            OrderId = n.OrderId,
            CustomerId = n.CustomerId,
            Message = n.Message,
            EventType = n.EventType,
            CreatedAt = n.CreatedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var n = await _context.Notifications.FindAsync(id);
        if (n == null) return NotFound();
        _context.Notifications.Remove(n);
        await _context.Notifications.Where(x => x.Id == id).ExecuteDeleteAsync();
        return NoContent();
    }
}
