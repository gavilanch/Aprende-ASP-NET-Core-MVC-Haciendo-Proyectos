namespace BlogMVC.Models
{
    public class EntradasConComentariosNegativosViewModel
    {
        public IEnumerable<EntradaConComentariosNegativosViewModel> Entradas { get; set; } = [];
    }

    public class EntradaConComentariosNegativosViewModel
    {
        public int Id { get; set; }
        public required string Titulo { get; set; }
        public int CantidadComentariosNegativos { get; set; }
    }

}
