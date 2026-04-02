using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Notifications.Data;
using Service_Notifications.Models;
using Service_Notifications.Services;
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
        private readonly IEmailService _emailService; // ---> A AJOUTER

        // Injection du contexte de base de données ET du client HTTP
        public NotificationsController(NotificationDbContext context, IHttpClientFactory httpClientFactory, IEmailService emailService)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _emailService = emailService; // ---> A AJOUTER
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

            // ---> 2. Appeler le service pour envoyer le vrai email
            try 
            {
                string sujet = $"Notification pour la réservation #{notification.ReservationId}";
                string corpsHtml = $"<h3>Bonjour,</h3><p>{notification.Message}</p>";

                // Notification.Destinataire doit contenir une vraie adresse email (ex: "client@domaine.com")
                await _emailService.SendEmailAsync(notification.Destinataire, sujet, corpsHtml);
                
                Console.WriteLine($"[SUCCÈS] Email envoyé à {notification.Destinataire}");
            }
            catch (Exception ex)
            {
                // En microservices, on gère les erreurs en évitant d'exposer les secrets réseau.
                // Vous pourriez décider de sauvegarder quand même la notification et la marquer comme "Échouée"
                return StatusCode(500, $"Erreur lors de l'envoi de l'email : {ex.Message}");
            }

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