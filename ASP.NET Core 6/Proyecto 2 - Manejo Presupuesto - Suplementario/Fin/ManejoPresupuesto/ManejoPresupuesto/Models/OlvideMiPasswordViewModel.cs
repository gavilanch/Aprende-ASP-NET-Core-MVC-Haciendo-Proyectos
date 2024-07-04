using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class OlvideMiPasswordViewModel
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El campo debe ser un correo electrónico válido")]
        public string Email { get; set; }
    }
}
