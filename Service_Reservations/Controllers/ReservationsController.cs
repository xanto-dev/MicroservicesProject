using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Reservations.Data;
using Service_Reservations.Models;
using System.Net.Http.Headers;

namespace Service_Reservations.Controllers
{
    [Authorize]
    [Route("api/reservations")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

      
        public ReservationsController(ReservationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }

        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation(Reservation reservation)
        {
            //Création du client HTTP
            var client = _httpClientFactory.CreateClient();

            //Transfert du Token JWT (Super important !)
            // recuperation du token que Swagger nous a envoyé et on le donne à notre client interne
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            
            string urlUtilisateurs = $"http://localhost:5001/api/utilisateurs/{reservation.UtilisateurId}";
            string urlRessources = $"http://localhost:5002/api/ressources/{reservation.RessourceId}";

            // verification de l'existence de l'utilisateur et de la ressource avant de créer la réservation
            var reponseUtilisateur = await client.GetAsync(urlUtilisateurs);
            if (!reponseUtilisateur.IsSuccessStatusCode)
            {
                return BadRequest($"Impossible de créer la réservation : L'utilisateur avec l'ID {reservation.UtilisateurId} n'existe pas.");
            }

            //verfication de l'existence de la ressource
            var reponseRessource = await client.GetAsync(urlRessources);
            if (!reponseRessource.IsSuccessStatusCode)
            {
                return BadRequest($"Impossible de créer la réservation : La ressource avec l'ID {reservation.RessourceId} n'existe pas.");
            }

            //sauvegarde de la réservation
            reservation.Statut = "En attente";
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}