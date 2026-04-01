using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Paiement.Data;
using Service_Paiement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service_Paiement.Controllers
{
    [Authorize] // Exige le Token JWT
    [Route("api/paiement")] // Correspond à ocelot.paiement.json
    [ApiController]
    public class PaiementsController : ControllerBase
    {
        private readonly PaiementDbContext _context;

        public PaiementsController(PaiementDbContext context)
        {
            _context = context;
        }

        // GET: api/paiement
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Paiement>>> GetPaiements()
        {
            return await _context.Paiements.ToListAsync();
        }

        // GET: api/paiement/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Paiement>> GetPaiement(int id)
        {
            var paiement = await _context.Paiements.FindAsync(id);

            if (paiement == null)
            {
                return NotFound();
            }

            return paiement;
        }

        // POST: api/paiement
        [HttpPost]
        public async Task<ActionResult<Paiement>> PostPaiement(Paiement paiement)
        {
            // 1. Initialisation
            paiement.DateTransaction = DateTime.UtcNow;

            if (paiement.Montant <= 0)
            {
                return BadRequest("Le montant doit être supérieur à zéro.");
            }

            // 2. Traitement du paiement avec Stripe
            try
            {
                var options = new Stripe.ChargeCreateOptions
                {
                    // Stripe fonctionne en centimes : on multiplie par 100
                    Amount = (long)(paiement.Montant * 100),
                    Currency = "cad", // Dollars canadiens
                    Description = $"Paiement pour la réservation ID: {paiement.ReservationId}",

                    // tok_visa est un token de test fourni par Stripe pour simuler un succès
                    Source = "tok_visa"
                };

                var service = new Stripe.ChargeService();
                Stripe.Charge charge = await service.CreateAsync(options);

                // Vérification du résultat renvoyé par Stripe
                if (charge.Status == "succeeded")
                {
                    paiement.Statut = "Complété";
                }
                else
                {
                    paiement.Statut = "Échoué";
                }
            }
            catch (Stripe.StripeException ex)
            {
                // Gestion des erreurs Stripe (carte refusée, problème de clé d'API, etc.)
                paiement.Statut = "Échoué";
                return BadRequest(new { Message = "Erreur de paiement avec Stripe.", Detail = ex.Message });
            }

            // 3. Sauvegarde dans la base de données locale
            _context.Paiements.Add(paiement);
            await _context.SaveChangesAsync();

            // 4. Retour du résultat au client
            return CreatedAtAction(nameof(GetPaiement), new { id = paiement.Id }, paiement);
        }

        // PUT: api/paiement/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaiement(int id, Paiement paiement)
        {
            if (id != paiement.Id)
            {
                return BadRequest("L'ID fourni ne correspond pas à la transaction.");
            }

            _context.Entry(paiement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaiementExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/paiement/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaiement(int id)
        {
            var paiement = await _context.Paiements.FindAsync(id);
            if (paiement == null)
            {
                return NotFound();
            }

            _context.Paiements.Remove(paiement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PaiementExists(int id)
        {
            return _context.Paiements.Any(e => e.Id == id);
        }
    }
}