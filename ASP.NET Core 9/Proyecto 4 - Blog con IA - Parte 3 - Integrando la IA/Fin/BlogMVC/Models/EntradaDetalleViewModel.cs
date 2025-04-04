namespace BlogMVC.Models
{
    public class EntradaDetalleViewModel
    {
        public int Id { get; set; }
        public required string Titulo { get; set; }
        public required string Cuerpo { get; set; }
        public string? PortadaUrl { get; set; }
        public required string EscritoPor { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public bool MostrarBotonEdicion { get; set; }
        public bool EntradaBorrada { get; set; }
        public IEnumerable<ComentarioViewModel> Comentarios { get; set; } = [];
    }
}
