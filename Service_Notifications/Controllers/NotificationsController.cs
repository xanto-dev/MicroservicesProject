using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Notifications.Data;
using Service_Notifications.Models;

namespace Service_Notifications.Controllers
{
    [Authorize] // On sécurise l'accès
    [Route("api/notifications")] // Correspond à Ocelot
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationDbContext _context;

        public NotificationsController(NotificationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            return await _context.Notifications.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            return notification;
        }

        [HttpPost]
        public async Task<ActionResult<Notification>> PostNotification(Notification notification)
        {
            notification.DateEnvoi = DateTime.UtcNow;

            // --- SIMULATION DE L'ENVOI ---
            // Dans un vrai projet, on utiliserait SendGrid ou Twilio ici.
            // Pour le TP, on affiche simplement un message dans la console du serveur.
            Console.WriteLine("\n==================================================");
            Console.WriteLine($"[ENVOI FICTIF] Type: {notification.Type}");
            Console.WriteLine($"À: {notification.Destinataire}");
            Console.WriteLine($"Message: {notification.Message}");
            Console.WriteLine("==================================================\n");

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}