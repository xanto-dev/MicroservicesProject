using Microsoft.EntityFrameworkCore;
using Service_Utilisateurs.Models;

namespace Service_Utilisateurs.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
    }
}