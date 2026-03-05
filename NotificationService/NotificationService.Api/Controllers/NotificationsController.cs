using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Data;
using NotificationService.Api.Models;

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

    // GET: api/notifications
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var notifications = await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
        return Ok(notifications);
    }

    // GET: api/notifications/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();
        return Ok(notification);
    }

    // POST: api/notifications
    [HttpPost]
    public async Task<IActionResult> Create(Notification notification)
    {
        notification.CreatedAt = DateTime.UtcNow;
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = notification.Id }, notification);
    }

    // DELETE: api/notifications/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}