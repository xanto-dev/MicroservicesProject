using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Notifications.Data;
using Service_Notifications.Models;
using System.Net.Http.Headers;

namespace Service_Notifications.Controllers
{
    [Authorize]
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        // Injection du contexte de base de données ET du client HTTP
        public NotificationsController(NotificationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
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
            // 1. Préparation du client HTTP et transfert du JWT
            var client = _httpClientFactory.CreateClient();
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // --- ⚠️ REMPLACEZ PAR LE VRAI PORT DU SERVICE RÉSERVATIONS ---
            string urlReservations = $"http://localhost:5003/api/reservations/{notification.ReservationId}";

            // 2. LA VÉRIFICATION : Est-ce que cette réservation existe ?
            var reponseReservation = await client.GetAsync(urlReservations);

            if (!reponseReservation.IsSuccessStatusCode)
            {
                // Si la réservation n'existe pas, on bloque la notification
                return BadRequest($"Impossible d'envoyer la notification : La réservation {notification.ReservationId} n'existe pas dans le système.");
            }

            // 3. Si on arrive ici, la réservation existe ! On procède à l'enregistrement.
            notification.DateEnvoi = DateTime.UtcNow;

            // Simulation de l'envoi dans la console
            Console.WriteLine("=============================================");
            Console.WriteLine($"[SUCCÈS] Nouvelle Notification Validée !");
            Console.WriteLine($"Réservation ID : {notification.ReservationId}");
            Console.WriteLine($"Destinataire   : {notification.Destinataire}");
            Console.WriteLine($"Message        : {notification.Message}");
            Console.WriteLine("=============================================\n");

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}