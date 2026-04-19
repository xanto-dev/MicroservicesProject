namespace Service_Reservations.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        
        public int UtilisateurId { get; set; }
        public int RessourceId { get; set; }

        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }

        
        public string Statut { get; set; } = "En attente";

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    }
}