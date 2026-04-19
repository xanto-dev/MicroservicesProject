namespace Service_Paiement.Models
{
    public class Paiement
    {
        public int Id { get; set; }

        
        public int ReservationId { get; set; }

        public decimal Montant { get; set; }

        
        public string Methode { get; set; } = "Stripe";

        
        public string Statut { get; set; } = "En attente";

        public DateTime DateTransaction { get; set; } = DateTime.UtcNow;
    }
}