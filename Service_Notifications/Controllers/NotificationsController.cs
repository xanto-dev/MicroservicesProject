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
        private readonly IEmailService _emailService;

        // Injection du contexte de base de données ET du client HTTP
        public NotificationsController(NotificationDbContext context, IHttpClientFactory httpClientFactory, IEmailService emailService)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _emailService = emailService;
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
            // Préparation du client HTTP et transfert du JWT
            var client = _httpClientFactory.CreateClient();
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

          
            string urlReservations = $"http://localhost:5003/api/reservations/{notification.ReservationId}";

            
            var reponseReservation = await client.GetAsync(urlReservations);

            if (!reponseReservation.IsSuccessStatusCode)
            {
                // Si la réservation n'existe pas, on bloque la notification
                return BadRequest($"Impossible d'envoyer la notification : La réservation {notification.ReservationId} n'existe pas dans le système.");
            }

            // en cas de succès, on peut continuer à préparer la notification
            notification.DateEnvoi = DateTime.UtcNow;

            // appel au service d'email pour envoyer la notification par email
            try
            {
                string sujet = $"Notification pour la réservation #{notification.ReservationId}";
                string corpsHtml = $"<h3>Bonjour,</h3><p>{notification.Message}</p>";

                 
                await _emailService.SendEmailAsync(notification.Destinataire, sujet, corpsHtml);
                
                Console.WriteLine($"[SUCCÈS] Email envoyé à {notification.Destinataire}");
            }
            catch (Exception ex)
            {
                
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