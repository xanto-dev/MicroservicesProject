namespace Service_Notifications.Models
{
    public class Notification
    {
        public int Id { get; set; }

 
        public int ReservationId { get; set; }

        
        public string Destinataire { get; set; } = string.Empty;

        
        public string Type { get; set; } = "Email";

        public string Message { get; set; } = string.Empty;

        public DateTime DateEnvoi { get; set; } = DateTime.UtcNow;
    }
}