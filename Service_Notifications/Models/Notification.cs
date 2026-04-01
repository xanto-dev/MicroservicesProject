namespace Service_Notifications.Models
{
    public class Notification
    {
        public int Id { get; set; }

        // Pour savoir à quelle réservation cette notification est liée
        public int ReservationId { get; set; }

        // Adresse email ou numéro de téléphone
        public string Destinataire { get; set; } = string.Empty;

        // "Email" ou "SMS"
        public string Type { get; set; } = "Email";

        public string Message { get; set; } = string.Empty;

        public DateTime DateEnvoi { get; set; } = DateTime.UtcNow;
    }
}