using System.ComponentModel.DataAnnotations;

namespace BlogMVC.Entidades
{
    public class Comentario
    {
        public int Id { get; set; }
        public int EntradaId { get; set; }
        public Entrada? Entrada { get; set; }
        [Required]
        public string Cuerpo { get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; }
        public string? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public bool Borrado { get; set; }
    }
}
