using Microsoft.EntityFrameworkCore;
using Service_Reservations.Models;

namespace Service_Reservations.Data
{
    public class ReservationDbContext : DbContext
    {
        public ReservationDbContext(DbContextOptions<ReservationDbContext> options) : base(options)
        {
        }

        public DbSet<Reservation> Reservations { get; set; }
    }
}