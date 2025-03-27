namespace BlogMVC.Models
{
    public class UsuariosListadoViewModel
    {
        public IEnumerable<UsuarioViewModel> Usuarios { get; set; } = [];
        public string? Mensaje { get; set; }
    }
}
