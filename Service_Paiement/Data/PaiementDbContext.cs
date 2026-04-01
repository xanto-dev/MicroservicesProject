using Microsoft.EntityFrameworkCore;
using Service_Paiement.Models;

namespace Service_Paiement.Data
{
    public class PaiementDbContext : DbContext
    {
        public PaiementDbContext(DbContextOptions<PaiementDbContext> options) : base(options)
        {
        }

        public DbSet<Paiement> Paiements { get; set; }
    }
}