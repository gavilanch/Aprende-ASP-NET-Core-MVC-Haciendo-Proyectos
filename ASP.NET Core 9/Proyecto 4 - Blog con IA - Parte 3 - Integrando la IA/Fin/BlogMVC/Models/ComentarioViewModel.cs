namespace BlogMVC.Models
{
    public class ComentarioViewModel
    {
        public int Id { get; set; }
        public required string Cuerpo { get; set; }
        public required string EscritoPor { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public bool MostrarBotonBorrar { get; set; }
    }
}
