using Microsoft.EntityFrameworkCore;
using Service_Notifications.Models;

namespace Service_Notifications.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }
    }
}