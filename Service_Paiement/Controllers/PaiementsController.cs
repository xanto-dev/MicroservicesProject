using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Paiement.Data;
using Service_Paiement.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace Service_Paiement.Controllers
{
    [Authorize]
    [Route("api/paiement")]
    [ApiController]
    public class PaiementsController : ControllerBase
    {
        private readonly PaiementDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        // Injection de IHttpClientFactory
        public PaiementsController(PaiementDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Paiement>>> GetPaiements()
        {
            return await _context.Paiements.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Paiement>> GetPaiement(int id)
        {
            var paiement = await _context.Paiements.FindAsync(id);
            if (paiement == null) return NotFound();
            return paiement;
        }

        [HttpPost]
        public async Task<ActionResult<Paiement>> PostPaiement(Paiement paiement)
        {
            paiement.DateTransaction = DateTime.UtcNow;

            if (paiement.Montant <= 0)
            {
                return BadRequest("Le montant doit être supérieur à zéro.");
            }

            // 1. PRÉPARATION DU CLIENT HTTP ET DU TOKEN JWT
            var client = _httpClientFactory.CreateClient();
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // --- ⚠️ REMPLACEZ PAR VOS VRAIS PORTS LOCAUX ⚠️ ---
            string urlReservations = $"http://localhost:5003/api/reservations/{paiement.ReservationId}";
            string urlNotifications = $"http://localhost:5005/api/notifications";

            // 2. VÉRIFICATION DE LA RÉSERVATION
            var reponseReservation = await client.GetAsync(urlReservations);
            if (!reponseReservation.IsSuccessStatusCode)
            {
                return BadRequest($"Le paiement est refusé : La réservation avec l'ID {paiement.ReservationId} n'existe pas.");
            }

            // On lit les détails de la réservation pour pouvoir la modifier plus tard
            var reservationJson = await reponseReservation.Content.ReadAsStringAsync();
            var reservationOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            // Note: On utilise dynamic ici pour simplifier, car on n'a pas la classe Reservation dans ce projet
            var reservationData = JsonSerializer.Deserialize<JsonElement>(reservationJson, reservationOptions);

            // 3. TRAITEMENT STRIPE
            try
            {
                var options = new Stripe.ChargeCreateOptions
                {
                    Amount = (long)(paiement.Montant * 100),
                    Currency = "cad",
                    Description = $"Paiement pour la réservation ID: {paiement.ReservationId}",
                    Source = "tok_visa"
                };

                var service = new Stripe.ChargeService();
                Stripe.Charge charge = await service.CreateAsync(options);

                if (charge.Status == "succeeded")
                {
                    paiement.Statut = "Complété";

                    // 4. MISE À JOUR DE LA RÉSERVATION (Statut -> "Payé")
                    var reservationMiseAJour = new
                    {
                        id = reservationData.GetProperty("id").GetInt32(),
                        utilisateurId = reservationData.GetProperty("utilisateurId").GetInt32(),
                        ressourceId = reservationData.GetProperty("ressourceId").GetInt32(),
                        dateDebut = reservationData.GetProperty("dateDebut").GetDateTime(),
                        dateFin = reservationData.GetProperty("dateFin").GetDateTime(),
                        statut = "Payé" // Le nouveau statut !
                    };

                    var contentPut = new StringContent(JsonSerializer.Serialize(reservationMiseAJour), Encoding.UTF8, "application/json");
                    await client.PutAsync(urlReservations, contentPut);

                    // 5. ENVOI DE LA NOTIFICATION (Bonus 5%)
                    var notification = new
                    {
                        reservationId = paiement.ReservationId,
                        // On récupère le courriel de l'utilisateur connecté directement depuis son Token JWT !
                        destinataire = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "client@uqar.ca",
                        type = "Email",
                        message = $"Votre paiement de {paiement.Montant}$ a été accepté. Votre réservation #{paiement.ReservationId} est confirmée !"
                    };

                    var contentPost = new StringContent(JsonSerializer.Serialize(notification), Encoding.UTF8, "application/json");
                    await client.PostAsync(urlNotifications, contentPost);
                }
                else
                {
                    paiement.Statut = "Échoué";
                }
            }
            catch (Stripe.StripeException ex)
            {
                paiement.Statut = "Échoué";
                return BadRequest(new { Message = "Erreur de paiement avec Stripe.", Detail = ex.Message });
            }

            // 6. SAUVEGARDE DU PAIEMENT
            _context.Paiements.Add(paiement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPaiement), new { id = paiement.Id }, paiement);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaiement(int id)
        {
            var paiement = await _context.Paiements.FindAsync(id);
            if (paiement == null) return NotFound();

            _context.Paiements.Remove(paiement);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}