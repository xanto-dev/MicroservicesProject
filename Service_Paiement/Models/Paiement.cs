namespace Service_Paiement.Models
{
    public class Paiement
    {
        public int Id { get; set; }

        // Lien avec le microservice de Réservations
        public int ReservationId { get; set; }

        public decimal Montant { get; set; }

        // Ex: "Stripe", "PayPal", "Carte de crédit"
        public string Methode { get; set; } = "Stripe";

        // Ex: "En attente", "Complété", "Échoué"
        public string Statut { get; set; } = "En attente";

        public DateTime DateTransaction { get; set; } = DateTime.UtcNow;
    }
}