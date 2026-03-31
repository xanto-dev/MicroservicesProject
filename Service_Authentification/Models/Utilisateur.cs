namespace Service_Authentification.Models
{
    public class Utilisateur
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string MotDePasseHash { get; set; } = string.Empty;
    }
}