using Microsoft.EntityFrameworkCore;
using Service_Ressources.Models;

namespace Service_Ressources.Data
{
    public class RessourceDbContext : DbContext
    {
        public RessourceDbContext(DbContextOptions<RessourceDbContext> options) : base(options)
        {
        }

        public DbSet<Ressource> Ressources { get; set; }
    }
}