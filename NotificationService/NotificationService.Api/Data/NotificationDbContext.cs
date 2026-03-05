using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Models;

namespace NotificationService.Api.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();
}
