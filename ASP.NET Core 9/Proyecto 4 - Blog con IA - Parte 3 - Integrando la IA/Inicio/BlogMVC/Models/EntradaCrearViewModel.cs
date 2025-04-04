using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BlogMVC.Models
{
    public class EntradaCrearViewModel
    {
        [Required(ErrorMessage = "El {0} es requerido")]
        public required string Titulo { get; set; }
        [Required(ErrorMessage = "El {0} es requerido")]
        public required string Cuerpo { get; set; }
        [DisplayName("Imagen Portada")]
        public IFormFile? ImagenPortada { get; set; }
    }
}
