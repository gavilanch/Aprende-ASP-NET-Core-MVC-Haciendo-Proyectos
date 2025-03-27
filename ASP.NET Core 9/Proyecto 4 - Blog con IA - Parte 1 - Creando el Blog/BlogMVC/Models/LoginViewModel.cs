using System.ComponentModel.DataAnnotations;

namespace BlogMVC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El campo debe ser un correo electrónico válido")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Display(Name = "Recuérdame")]
        public bool Recuerdame { get; set; }

        public string? UrlRetorno { get; set; }

    }
}
