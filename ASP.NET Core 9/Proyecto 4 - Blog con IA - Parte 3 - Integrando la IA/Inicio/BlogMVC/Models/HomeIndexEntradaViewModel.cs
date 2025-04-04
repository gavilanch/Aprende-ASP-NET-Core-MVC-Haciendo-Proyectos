namespace BlogMVC.Models
{
    public class HomeIndexEntradaViewModel
    {
        public int Id { get; set; }
        public required string Titulo { get; set; }
        public string? PortadaUrl { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}
