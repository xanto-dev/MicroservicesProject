using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Ressources.Data;
using Service_Ressources.Models;

namespace Service_Ressources.Controllers
{
    [Authorize]
    [Route("api/ressources")]
    [ApiController]
    public class RessourcesController : ControllerBase
    {
        private readonly RessourceDbContext _context;

        public RessourcesController(RessourceDbContext context)
        {
            _context = context;
        }

        // GET api/ressources (Récupérer toutes les ressources)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ressource>>> GetRessources()
        {
            return await _context.Ressources.ToListAsync();
        }

        // GET api/ressources/5 (Récupérer une ressource par ID)
        [HttpGet("{id}")]
        public async Task<ActionResult<Ressource>> GetRessource(int id)
        {
            var ressource = await _context.Ressources.FindAsync(id);

            if (ressource == null)
            {
                return NotFound();
            }

            return ressource;
        }

        // POST api/ressources (Créer une nouvelle ressource)
        [HttpPost]
        public async Task<ActionResult<Ressource>> PostRessource(Ressource ressource)
        {
            _context.Ressources.Add(ressource);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRessource), new { id = ressource.Id }, ressource);
        }

        // PUT api/ressources/5 (Mettre à jour une ressource existante)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRessource(int id, Ressource ressource)
        {
            if (id != ressource.Id)
            {
                return BadRequest();
            }

            _context.Entry(ressource).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RessourceExists(id))
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

        // DELETE api/ressources/5 (Supprimer une ressource)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRessource(int id)
        {
            var ressource = await _context.Ressources.FindAsync(id);
            if (ressource == null)
            {
                return NotFound();
            }

            _context.Ressources.Remove(ressource);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RessourceExists(int id)
        {
            return _context.Ressources.Any(e => e.Id == id);
        }
    }
}