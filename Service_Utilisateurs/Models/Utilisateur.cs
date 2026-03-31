namespace Service_Utilisateurs.Models
{
    public class Utilisateur
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Pour la sécurité, on ne stockera que le hash du mot de passe
        public string MotDePasseHash { get; set; } = string.Empty;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    }
}