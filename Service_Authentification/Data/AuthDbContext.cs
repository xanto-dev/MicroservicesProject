using Microsoft.EntityFrameworkCore;
using Service_Authentification.Models;

namespace Service_Authentification.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
    }
}