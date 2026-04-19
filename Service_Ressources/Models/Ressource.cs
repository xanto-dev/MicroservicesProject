namespace Service_Ressources.Models
{
    public class Ressource
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int QuantiteDisponible { get; set; }
    }
}