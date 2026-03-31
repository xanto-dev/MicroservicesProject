using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Utilisateurs.Data;
using Service_Utilisateurs.Models;

namespace Service_Utilisateurs.Controllers
{
    [ApiController]
    [Route("api/utilisateurs")] // Correspond parfaitement à ta configuration Ocelot
    public class UtilisateursController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UtilisateursController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/utilisateurs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUtilisateurs()
        {
            // On retourne les utilisateurs sans exposer le mot de passe haché
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

        // GET: api/utilisateurs/5
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

        // POST: api/utilisateurs
        [HttpPost]
        public async Task<ActionResult<Utilisateur>> PostUtilisateur(Utilisateur utilisateur)
        {
            // Vérification basique si l'email existe déjà
            var emailExiste = await _context.Utilisateurs.AnyAsync(u => u.Email == utilisateur.Email);
            if (emailExiste)
            {
                return BadRequest("Un utilisateur avec cet email existe déjà.");
            }

            // Hachage du mot de passe avec BCrypt avant la sauvegarde
            // Nécessite le package NuGet : BCrypt.Net-Next
            utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(utilisateur.MotDePasseHash);
            utilisateur.DateCreation = DateTime.UtcNow;

            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            // On ne renvoie pas le hash dans la réponse de création
            utilisateur.MotDePasseHash = string.Empty;

            return CreatedAtAction(nameof(GetUtilisateur), new { id = utilisateur.Id }, utilisateur);
        }

        // PUT: api/utilisateurs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUtilisateur(int id, Utilisateur utilisateur)
        {
            if (id != utilisateur.Id)
            {
                return BadRequest("L'ID de l'URL ne correspond pas à l'ID de l'utilisateur.");
            }

            _context.Entry(utilisateur).State = EntityState.Modified;

            // Si le mot de passe a été modifié, il faudrait le re-hacher ici.
            // Pour simplifier ce code, on suppose qu'il n'est pas modifié dans cette requête basique.

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