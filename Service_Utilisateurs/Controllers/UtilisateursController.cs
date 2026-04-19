using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Utilisateurs.Data;
using Service_Utilisateurs.Models;

namespace Service_Utilisateurs.Controllers
{
    [ApiController]
    [Route("api/utilisateurs")]
    public class UtilisateursController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UtilisateursController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/utilisateurs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUtilisateurs()
        {
            // retourne une liste d'utilisateurs sans le mot de passe pour des raisons de sécurité
            var utilisateurs = await _context.Utilisateurs
                .Select(u => new
                {
                    u.Id,
                    u.Nom,
                    u.Prenom,
                    u.Email,
                    u.DateCreation
                })
                .ToListAsync();

            return Ok(utilisateurs);
        }

        // GET api/utilisateurs/5 (Récupérer un utilisateur par ID)
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUtilisateur(int id)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);

            if (utilisateur == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                utilisateur.Id,
                utilisateur.Nom,
                utilisateur.Prenom,
                utilisateur.Email,
                utilisateur.DateCreation
            });
        }

        // POST api/utilisateurs (Créer un nouvel utilisateur)
        [HttpPost]
        public async Task<ActionResult<Utilisateur>> PostUtilisateur(Utilisateur utilisateur)
        {
            // Vérification si l'email existe déjà
            var emailExiste = await _context.Utilisateurs.AnyAsync(u => u.Email == utilisateur.Email);
            if (emailExiste)
            {
                return BadRequest("Un utilisateur avec cet email existe déjà.");
            }

            // Hachage du mot de passe avec BCrypt avant la sauvegarde
       
            utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(utilisateur.MotDePasseHash);
            utilisateur.DateCreation = DateTime.UtcNow;

            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            
            utilisateur.MotDePasseHash = string.Empty;

            return CreatedAtAction(nameof(GetUtilisateur), new { id = utilisateur.Id }, utilisateur);
        }

        // PUT api/utilisateurs/5 (Mettre à jour un utilisateur existant)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUtilisateur(int id, Utilisateur utilisateur)
        {
            if (id != utilisateur.Id)
            {
                return BadRequest("L'ID de l'URL ne correspond pas à l'ID de l'utilisateur.");
            }

            _context.Entry(utilisateur).State = EntityState.Modified;

            // Si le mot de passe est modifié, hacher le nouveau mot de passe
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UtilisateurExists(id))
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

        // DELETE: api/utilisateurs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUtilisateur(int id)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur == null)
            {
                return NotFound();
            }

            _context.Utilisateurs.Remove(utilisateur);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UtilisateurExists(int id)
        {
            return _context.Utilisateurs.Any(e => e.Id == id);
        }
    }
}